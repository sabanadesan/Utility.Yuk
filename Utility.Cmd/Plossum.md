https://www.codeproject.com/Articles/19869/Powerful-and-simple-command-line-parsing-in-C

1 Introduction

1.1 Overview

Most applications written today accept command line arguments of some form. Sometimes we only need to parse the command line for file names that should be opened upon starting the applications, and at other times we need to process a large amount of various options controlling the way our application will execute. Most of us have written some code to process the command line arguments passed to an application at some point during our careers. This is tedious work, and it is easy to make mistakes that lead to errors when further executing our application because we forgot to validate some combination of options or values.

Several libraries exist that provide methods to perform the parsing of the command line and allow us to retrieve the specified options in a (sometimes not so) simple way. In UN*X environments, that usually utilize command line arguments quite heavily, the getopt[2] library. As I discovered recently there is actually a port of this available for C# as well, called "GNU Getopt .NET"[5]. Several projects are also available from Code Project. However, either the libraries available were not full featured enough for my preference, or I simply did not like the syntax they provided for using them, so I decided to roll my own command line parser. This article describes this library I made in a tutorial format which highlights most of its features.

1.2 Background

The format of how command line options (or switches) should be specified to a program varies between systems and applications. The traditional way in UN*X systems is to use single letter switches that are introduced via a - (hyphen); e.g. ls -l -F -a. These type of options are commonly referred to as "flags". Multiple flags may also be combined or grouped into one, so the example above could be rewritten as ls -lFa. There is also another way to specify options using the GNU "long options" which are introduced via -- and are typically one or more whole words, e.g. ls --long --classify --all. Arguments to long options are typically provided with = as in ls --block-size=1024. There are also programs that use long options with single dashes, such as mplayer -nosound. The GNU -- is also used to terminate the list of options, which may be useful if you need to specify e.g. a file name that starts with a dash.

On MS-DOS options were traditionally single characters introduced via / a slash, e.g. dir /w /p /a:s. Here the colon character serves the same purpose as = in the examples above. See [1] for more information on the history and traditions of command line arguments.

These days these styles are mixed heavily, and many windows programs today also accept options prefixed with a single dash, and arguments to options, long or short, can commonly be assigned without the use of the = or : characters. I wanted a library that could support all of these styles and be configurable as to which styles to allow and how they should be interpreted.

Furthermore I wanted to avoid clumsy code to specify which command line options were allowed and required, to retrieve and validate the supplied arguments etc. I also wanted usage information for display in a help text to be automatically generated from the specifications of command line options. An article on Code Project titled "Automatic Command Line Parsing in C#"[3] used an ingenious way to specify the available options to a program using .NET attributes. This was a very clean, simple to understand, read and manage way of specifying the available options and various parameters for each option. However the parser provided in that article lacked many features that I wanted, so I decided to adapt the strategy of using attributes to specify the command line options available and created my own library for parsing the command line arguments; Plossum.CommandLine.

2 Requirements

This command line library is written in C# for the .NET 2.0 Framework. It is dependent on "The C5 Generic Collection Library for C# and CLI"[4], which is a great library that in my humble opinion should be integrated into the .NET framework.

3 A first example: Hello world!

Let's begin by looking at a very simple example of an application accepting only two options; -help and -name. If help is specified a help text describing the program should be printed to the console. If help is not specified the name option must be specified and it should be provided with the name of the user running the program. In this very simple example it would probably be more intuitive to provide the name without the use of the name option, but we use the option here for illustration purposes.

All classes belonging to this library are available in the Plossum.CommandLine namespace.

The first thing to do is create what we refer to as an "command line manager" class. This class is used for specifying the available options to the command line parser, and an instance of this object is what will receive the values of the options specified on the command line when the command line parser is run.

```
[CommandLineManager(ApplicationName="Hello World",
    Copyright="Copyright (c) Peter Palotas")]
class Options
{
   [CommandLineOption(Description="Displays this help text")]
   public bool Help = false;

   [CommandLineOption(Description = "Specifies the input file", MinOccurs=1)]
   public string Name
   {
      get { return mName; }
      set
      {
         if (String.IsNullOrEmpty(value))
            throw new InvalidOptionValueException(
                "The name must not be empty", false);
         mName = value;
      }
   }

   private string mName;
}
```

The code above illustrates the use of two (of the three available) attributes defined in the Plossum.CommandLine namespace, and also gives you a taste of how simple and clear it is to specify command line options using this attribute technique. The attribute CommandLineManager indicates that this object is a command line manager, and optionally allows you to specify more options that are global to the entire command line parsing. In this example we specify the name of the application and a copyright message. These strings will later be included in formatted messages generated by the command line parser. If we did not specify these options, they would default to the values specified in the AssemblyTitle and AssemblyCopyright attributes of the initially loaded assembly of the assembly.

The class contains one public field (yes, this should have been a property and not a public field had this been a real application, but for the sake of brevity we use a public field here) named Help, and one public property named Name. These both have the CommandLineOption attribute applied to them. This indicates that these members will be used as command line options. The Description parameter is used for specifying the help text for each option that can be displayed after the parser is constructed. The name of each option can be specified explicitly with the Name parameter, but in case this is not specified the name of the member (in this case Name and Help) are used. We also note that the setter method of the Name property throws an exception of type InvalidOptionValueException if the value is null or empty (it will never be passed as a null reference from the parser, but it may be an empty string though). Throwing this exception from a member when the value is being set by the parser will cause the parser to catch the exception and recognize and later report the error.

The parameter MinOccurs specified for the Name option indicates the minimum number of occurrences of this option that must be provided on the command line. For any non-array or non-collection type, this parameter must be either 0 or 1. Here we specify that the Name option must occur at least once, meaning that specifying it is mandatory.

Listing 3.2: Hello World - Main method

```
class Program
{
   static int Main(string[] args)
   {
      Options options = new Options();
      CommandLineParser parser = new CommandLineParser(options);
      parser.Parse();
      Console.WriteLine(parser.UsageInfo.GetHeaderAsString(78));

      if (options.Help)
      {
         Console.WriteLine(parser.UsageInfo.GetOptionsAsString(78));
         return 0;
      }
      else if (parser.HasErrors)
      {
         Console.WriteLine(parser.UsageInfo.GetErrorsAsString(78));
         return -1;
      }
      Console.WriteLine("Hello {0}!", options.Name);
      return 0;
   }
}
```

We start by creating an instance of our command line manager class Options, and then we create a new instance of CommandLineParser which is the class performing the actual parsing of the command line and pass it our newly created Options object. When creating the CommandLineParser it uses reflection on the object passed to it to create an internal representation of the options available and verifies that all parameters specified are correct. If anything is not correct here, an exception of type AttributeException will be thrown indicating the error. We don't catch exceptions of that type here, since such an exception indicates a programming error and should not occur when the application is deployed.

We then call the Parse() method of the CommandLineParser. This performs the actual parsing of the command line, and it sets any values in our Options instance that were specified as options on the command line.

The call to parser.UsageInfo.GetHeaderAsString(78) returns a string containing a typical "header" for the application, consisting of the application name, version number and copyright message, some of which we set using parameters in the previous listing. The argument 78 indicates the maximum width of the returned string, and the string will be fitted within this width using word wrapping.

Then we check if the Help option was specified, since if it was specified the Help member of our command line manager object will be set to true. If Help was not specified on the command line the field would not have been set at all by the parser, and hence it would still have the value false, since a boolean field defaults to false. If the Help option was specified, we print the string returned by parser.UsageInfo.GetOptionsAsString(78). Again the parameter indicates the width of the field in which to fit the string. The method called returns a string listing all options available to the program along with their descriptions as specified by the Description parameter of the CommandLineOptionAttribute.

If the Help option was not specified we check if there were any errors on the command line. The reason we do not check for this if Help was specified, is that the parser produces errors for missing options, and if specifying only the Help option the Name option would be missing and hence produce an error. The parser.UsageInfo.GetErrorsAsString(78) returns a string, again fitted within the specified with, listing the errors found on the command line. Note that if the set method of any property did throw the InvalidOptionValueException when called by the parser, this list will contain these errors as well.

If no errors were present, we know that the Name option has been specified and is non-empty, so we print a message saying "Hello" onto the console.

Listed below are a few test runs of the program with different sets of arguments and its output:

Listing 3.3: ex1.exe with no arguments
```
> ex1.exe
Hello World  version 1.0.0.0
Copyright (c) Peter Palotas

Errors:
   * Missing required option "Name". 
```

We can see from this run that providing no arguments to the application provides an error message saying that the Name option must be specified.

Listing 3.4: ex1.exe with the /Help argument

```
> ex1.exe /Help
Hello World  version 1.0.0.0
Copyright (c) Peter Palotas

Options:
   -Help      Displays this help text
   -Name      Specifies the input file
```

The Help option displays the available options together with their descriptions. Here we used the Windows style for specifying the option, but the Unix style would have worked as well, as we can see from our next test.

Listing 3.5: ex1.exe with -Name argument

```
> ex1.exe -Name=Peter Palotas
Hello World  version 1.0.0.0
Copyright (c) Peter Palotas

Hello Peter!
```

Here we see something we might not have expected. The last name, Palotas, is not output. The reason for this is that values and options are white space separated, meaning that the only thing being assigned to Name is "Peter". So what happened to "Palotas"? We didn't see any error message. Any arguments that are not options, introduced by one of the allowed characters (in this case either / or -) is regarded as an additional argument, and can be retrieved from the parser by examining the collection CommandLineParser.RemainingArguments. Since we aren't checking that in our program, the additional argument is simply ignored. So what if we want to specify a value, such as my name, that contains a white space to the application? The answer is we need to put it inside double quotes (additional quoting methods can be specified, as we explain in later sections of this tutorial). So:

Listing 3.6: ex1.exe with quoted argument

```
> ex1.exe -Name="Peter Palotas"
Hello World  version 1.0.0.0
Copyright (c) Peter Palotas

Hello Peter Palotas!
```

...gives us our desired result.

Listing 3.7: ex1.exe with erroneous arguments

```
> ex1.exe -n=23 -Name=""
Hello World  version 1.0.0.0
Copyright (c) Peter Palotas

Errors:
   * The name must not be empty
   * Unknown option "n"
```

This run shows the result of the InvalidOptionValueException being thrown by the setter of Name as well as specifying an undefined option on the command line. Feel free to play around with different combinations of options to this program and examine the results.

Here, we have shown one of the simplest examples imaginable for recognizing options, and it clearly illustrates the simplicity of using the Plossum command line parser. However it does not show much of the customization features available, nor how powerful this library really is. This will be shown in the next few sections of this tutorial.

4 Specifying the command line options

4.1 Attributes

Specifying what command line options are available, the text describing them, and the properties defining exactly how they work are done through the use of attributes as you saw in Section 3. There are three attributes provided by the Plossum command line library; the CommandLineManagerAttribute, the CommandLineOptionAttribute, and the CommandLineOptionGroupAttribute.

4.1.1 CommandLineManagerAttribute

This attribute is applied to the class which will act as the command line manager, i.e. the class defining the command line attributes available to the program, and defining the object that will receive the values of the options specified on the command line.

4.1.1.1 Descriptive parameters

This attribute contains four descriptive properties; ApplicationName, Copyright, Version, and Description. All these attributes default to the corresponding attribute defined on the assembly defining the executable file that was run, so setting them explicitly in this attribute is normally not necessary.

4.1.1.2 Specifying the option styles enabled

This attribute also allows you to specify the option styles that should be recognized by the parser. This is done using the EnabledOptionStyles property which can be set to any combination of the flags from the enum OptionStyles. For more information, see 4.2.

4.1.1.3 Case sensitivity of option names

You have the option to specify whether option names are case sensitive or not, through the IsCaseSensitive property. The default is that options are not case sensitive.

4.1.1.4 Require assignment characters or make them optional

Finally the RequireExplicitAssignment parameter indicates whether the available assignment characters must be used for assigning a value to an option. If this is set to false (the default), values can also be assigned to options by delimiting them from the option name with a space, e.g. "ex1.exe /name Peter". If set to true the example above would generate an error saying "missing value for option 'name"', since the assignment character was not specified. Hence setting this to true requires all values to be assigned using an available assignment character, e.g. "ex1.exe /name:Peter". Note that this parameter is also available for option groups and options themselves, and setting it to a specific value in this attribute merely acts as the default for any options specified, and can be overridden on an individual option by option basis or for entire option groups if so desired.

4.1.2 CommandLineOptionGroupAttribute

An option group is a logical grouping of options. This serves mainly two purposes. One purpose is to group the options into groups containing functionally similar options, since when the usage text is retrieved from the command line parser the options are also grouped according to the grouping specified. The CommandLineOptionGroupAttribute may be applied only to a class, and only to a class that also has the CommandLineManager attribute.

4.1.2.1 Required Parameters

The Id property of this attribute is required and is set in the constructor. It must be unique among the option groups defined for a single command line manager class. It is used to refer to this group by options, see 4.1.3.2.

4.1.2.2 Descriptive Parameters

The group lets you specify a Name and a Description that will serve as a header for the options contained in that group in the usage description text. If the Name parameter is not specified, the default is to use the Id of the group.

4.1.2.3 Group requirements

Another purpose is to place occurrence requirements on groups of options, for an example requiring that at least one of the options in the group must be specified. This is done through the Require parameter which is an enumeration of type OptionGroupRequirement. This enumeration contains the following attributes with their specified meaning:

None 
This is the default, and it means that no restrictions are placed on the options in this group (other than those explicitly specified for each option)
AtMostOne 
This indicates that at most one of the options contained within this group must be specified. It means that either none of the options in the group, or one of the options in the group may be specified, but no more.
AtLeastOne 
This indicates that at least one of the options contained within this group must be specified, i.e. one or more options from this group must be specified.
ExactlyOne 
This indicates that exactly one option from this group must be specified, no more and no less.
All 
Indicates that all options from this group must be specified on the command line.

4.1.2.4 Make assignment characters required or optional

Just like the CommandLineManagerAttribute this attribute contains a parameter named RequireExplicitAssignment. Its function is the same as that for the CommandLineManagerAttribute, except it only affects the options that belong to this group. Individual options can still choose to override this. See 4.1.1.4 for more information.

4.1.3 CommandLineOptionAttribute

This attribute can be applied to fields, properties and methods of a class tagged with the CommandLineManagerAttribute. There are restrictions on the types the members to which it can be applied though, see 4.3.

4.1.3.1 Descriptive parameters
There are only two descriptive properties for this attribute, namely; Name and Description. The Name property denotes the name of the option as it should be specified on the command line. If this property is not explicitly specified, the name of the option will be the same as the name of the member to which the attribute is applied. The Description property specifies a description that is listed together with the option name(s) in the usage description generated by the command line parser.

4.1.3.2 Adding an option to an option group
The GroupId property can be set to the id of an option group, and will make this option a member of that group. See 4.1.2 for more information. The default is null, meaning this option does not belong to any group.

4.1.3.3 Restricting the number of times an option can be specified

The MinOccurs and MaxOccurs parameters specifies the cardinality of the option, i.e. how many times the option may (or must) be specified on the command line. MinOccurs determines the minimum number of times that the option must be specified, and MaxOccurs determines the maximum number of times the option may be specified. If MaxOccurs is set to zero, this has the special meaning that no upper bound on the number of times the option may be specified on the command line is in place. Note that for any non-array or non-collection member (with the exception of methods), MinOccurs must be set to either 0 or 1, and defaults to 0, and MaxOccurs must be set to 1.

4.1.3.4 Restricting the allowable range of values for numerical types

The MinValue and MaxValue parameters apply to numerical types only, and specify the minimum and maximum values allowed for the option respectively. If set to null they default to the largest or smallest value available for the type of the member to which the attribute is applied respectively. The default is null for both parameters.

4.1.3.5 Specifying aliases for an option

The Aliases property names aliases for this option, i.e. other names with which this option may be referred to. This is a string property, and multiple names are separated by a space or a comma. This is useful for specifying e.g. a long option name as an alias for a short name or similar. Any alias specified here must be unique among the aliases and option names in the current command line manager.

4.1.3.6 Prohibiting specific options from being specified together

Some options available to applications may be mutually exclusive, meaning that they must not both be specified on the command line. There are two ways to accomplish this using this library depending on your needs. One is already covered in 4.1.2.3. The other method is using the Prohibits parameter of the CommandLineOptionAttribute. This parameter accepts a comma delimited string of option names that the option in question prohibits. This means that as soon as this option is specified on the command line, none of the attributes in the Prohibits list must also be present. If option a is prohibited by option b, then option b is also implicitly prohibited by option a, so there is no need to specify this explicitly (although you may if you wish).

4.1.3.7 Special handling of boolean options

How boolean options are interpreted and their values set can be specified through the BoolFunction parameter. This parameter accepts an enum of the BoolFunction type. The following values and their respective meanings are available:

Value This makes the boolean option behave just like any other option, meaning that it expects (and requires) a value to be assigned to it on the command line. The value must be one of Bool.TrueString or Bool.FalseString disregarding case.
TrueIfPresent This is the default behavior for boolean options. It means that the option does not accept a value on the command line. Instead, if the option is present on the command line, the value of the member to which the attribute defining the option is applied will be set to the value true. As usual the value of the member will not be set at all by the CommandLineParser if the option is not specified on the command line.
FalseIfPresent 
This makes the option behave the same way as if TrueIfPresent was specified, with the difference that it sets the value of the member to which the attribute was applied to false if the option is specified on the command line.
UsePrefix 
This flag is only valid if the Group option style is specified (see 4.2). It means that the option does not accept a value, but instead the character introducing the option (the prefix) will determine what value the option will get. If the option is specified with a dash (-), the option is set to false and if the option is specified with a plus (+) the option is set to true.

4.1.3.8 Make assignment characters required or optional

This attribute also contains the RequireExplicitAssignment parameter, which overrides the setting specified in the group or manager to which the option belongs. It has the same meaning as described in 4.1.1.4.

4.1.3.9 Providing a default value

Providing a normal default value for the option, i.e. the value the member to which this attribute is applied should receive if the option is not specified on the command line is done by initializing the member with the required value in the constructor of the manager class. However, there is a parameter named DefaultAssignmentValue of this attribute which accepts either a string that contains a parseable value of the type of the member or a value of the same type as the member. Setting this parameter to anything other than a null reference allows the option in question to be specified without assigning a value to it. In case the option is specified on the command line without a value, the option will be set to the value specified in this parameter. Please note that in case the option is not specified on the command line, the member still will not be set by the parser and this value will have no meaning. Also note that setting this parameter does not forbid or prevent the user from specifying a different value on the command line, it merely provides a default value should the user opt not to provide one.

This parameter may only be set to something other than a null reference (meaning no default value is present) if RequireExplicitAssignment (see 4.1.3.8) is set to true.

4.2 Option Styles

As mentioned in Section 1 there are several "styles" for introducing options and specifying values belonging to those options on the command line. The Plossum command line library provides support for many combinations of these styles through the use of the EnabledOptionStyles parameter of the CommandLineManagerAttribute and the OptionStyles flags enumeration. This enumeration contains flags describing a number of styles, and may be specified in the EnabledOptionStyles property to select exactly which styles are allowed and recognized by the parser.

The styles available are:

None This denotes no styles enabled. The parser will automatically set itself to this style upon encountering the end of options switch (--).
Windows 
This denotes the "Windows style" of options, meaning that options are introduced on the command line using the forward slash (/) character. Option names are one or more characters in length and can not be grouped (see below). Values to these options can (by default) be assigned using either the colon (:) character, the equals (=) character or by delimiting the option name from the value with a space (unless explicitly prohibited). This flag is specified by default.
LongUnix 
This denotes the "GNU longopt style" of options, meaning that options are introduced on the command line using a double dash (--). Option names are one or more characters in length and are never grouped (see below). Values to these options can (by default) be assigned using either the equals (=) character or by delimiting the option name from the value with a space (unless explicitly prohibited).
ShortUnix 
This denotes the "short name UNIX style" of options, meaning that options are introduced on the command line using a single dash (-). Option names are one or more characters in length, depending on whether the Group flag is also specified. If the Group flag is also specified, option names introduced by a single dash can only be one character in length, otherwise they too can contain any number of characters. Values to these options are assigned the same way as for LongUnix (see above). This flag is specified by default.
File 
This allows so called "list files" or "option files" to be specified on the command line. These are introduced by the @-character immediately followed by a file name. If this style is enabled, and such an "option" is encountered on the command line, the command line parser will open this file and scan the contents of that file for more options or other arguments, following the same conventions as those options specified on the command line. This flag is specified by default.
Plus 
This flag implies the ShortUnix flag, meaning that enabling this flag also enables the ShortUnix flag. It allows options of the short UNIX style to be prefixed either by a dash (-) or by a plus (+) character. This has meaning only for boolean values, and allows the prefix (dash or plus) to specify the value that the boolean option will be set to.
Group 
This flag implies the ShortUnix flag, meaning that enabling this flag also enables the ShortUnix flag. It enables the grouping of options specified in the "short name UNIX style", meaning that instead of writing e.g. "tar -x -v -z -f file.tar.gz" on the command line you may group any single character options and instead write "tar -xvzf file.tar.gz", and this will be interpreted the same way. Please note that grouping works only for options specified using the "short name UNIX style" (prefixed with either a dash or a plus if that flag is also enabled), and prevents you from specifying any multicharacter names with the single dash.
Unix 
This is a combination flag supplied for convenience only and is the same as specifying both ShortUnix and LongUnix
All 
This is a combination of all available styles.
These flags may be freely combined using the logical or operator to specify the exact style that you want to allow your users to use for specifying the options. The only restriction is that you may not also use logical and to remove the ShortUnix flag when any flag implying it is also specified.

4.3 Supported Member Types

In short, the CommandLineOptionAttribute can be applied to members only of the built in types (except for object), arrays of these types, a System.Collections.ICollection, a System.Collections.Generic.ICollection<T> or a C5.ICollection<T> where T is one of these built in types. The supported built in types are:

bool
byte
sbyte
char
decimal
double
float
int
uint
long
ulong
short
ushort
string
enum

The attribute can be applied either to a public field of one of these types, or to a read/write property of one of these types. When the corresponding option is parsed by the command line parser, the field or property to which the attribute was applied will be set to the value specified on the command line, provided that the specified value was legal.

The attribute can also be applied to a public method accepting exactly one parameter that is of one of these built in types and is not an array type or collection type. In this case the method will be called whenever an option matching the attribute is found on the command line by the parser, and the value found will be passed to the method. This allows for a different handling than the ones provided by arrays and collections when multiple options of the same name are specified on the command line which may be desirable.

If the attribute is applied to an array or collection, the value of any option specified on the command line will be appended to the array or collection. This allows for a simple way of handling multiple options of the same name being specified.

If the attribute is applied to an enum, the value specified for the option must match one of the enumeration names defined in the enum, disregarding case. This means that using an enum where two enumeration members differ only by case is not allowed and will generate an exception in the constructor of CommandLineParser. (Having identifiers differing only by case is not recommended anyway, since not all .NET languages are case sensitive).

If a method or property setter throws an exception of the type InvalidOptionValueException when the parser attempts to assign a value to it, the parser will catch this exception and transform it into an error which can later be retrieved in the standard way for retrieving errors. This is useful for validating values in a convenient way, when the default validation supplied by the Plossum library is not enough. See Chapter 5.3.

4.4 A more advanced example

4.4.1 A simple 'tar'

The following listing is an example of a simple implementation of the command line parsing of a utility somewhat resembling the UNIX 'tar' utility. It demonstrates the use of aliases, group requirements, grouping of options, requiring explicit assignment and using collections to store values.

Listing 4.1: A simple 'tar'

```
namespace ex2 {
   [CommandLineManager(ApplicationName="Example 2",
       Copyright="Copyright (C) Peter Palotas 2007",
       EnabledOptionStyles=OptionStyles.Group | OptionStyles.LongUnix)]
   [CommandLineOptionGroup("commands", Name="Commands",
       Require=OptionGroupRequirement.ExactlyOne)]
   [CommandLineOptionGroup("options", Name="Options")]
   class Options
   {
      [CommandLineOption(Name="filter", RequireExplicitAssignment=true,
       Description="Specifies a filter on which files to include or exclude",
        GroupId="options")]
      public List<string> Filters
      {
         get { return mFilters; }
         set { mFilters = value; }
      }

      [CommandLineOption(Name="v", Aliases="verbose",
          Description="Produce verbose output", GroupId="options")]
      public bool Verbose
      {
         get { return mVerbose; }
         set { mVerbose = value; }
      }

      [CommandLineOption(Name="z", Aliases="use-compression",
        Description="Compress or decompress the archive", GroupId="options")]
      public bool UseCompression
      {
         get { return mUseCompression; }
         set { mUseCompression = value; }
      }

      [CommandLineOption(Name="c", Aliases="create",
          Description="Create a new archive", GroupId="commands")]
      public bool Create
      {
         get { return mCreate; }
         set { mCreate = value; }
      }

     [CommandLineOption(Name = "x", Aliases = "extract",
          Description = "Extract files from archive", GroupId = "commands")]
      public bool Extract
      {
         get { return mExtract;}
         set { mExtract = value;}
      }

      [CommandLineOption(Name="f", Aliases="file",
          Description="Specify the file name of the archive", MinOccurs=1)]
      public string FileName
      {
         get { return mFileName;}
         set { mFileName = value;}
      }

      [CommandLineOption(Name="h", Aliases="help",
          Description="Shows this help text", GroupId="commands")]
      public bool Help
      {
         get { return mHelp; }
         set { mHelp = value; }
      }

      private bool mHelp;
      private List<string> mFilters = new List<string>();
      private bool mCreate;
      private bool mExtract;
      private string mFileName;
      private bool mUseCompression;
      private bool mVerbose;
   }

   class Program
   {
      static int Main(string[] args)
      {
         Options options = new Options();
         CommandLineParser parser = new CommandLineParser(options);
         parser.Parse();

         if (options.Help)
         {
            Console.WriteLine(parser.UsageInfo.ToString(78, false));
            return 0;
         }
         else if (parser.HasErrors)
         {
            Console.WriteLine(parser.UsageInfo.ToString(78, true));
            return -1;
         }

         // No errors present and all arguments correct
         // Do work according to arguments
         return 0;
      }
   }
}
```

5 Using the command line parser

5.1 Constructing the parser

Plossum.CommandLine.CommandLineParser is the class used for performing the parsing of command lines, as you have already seen in the previous examples. Its constructor accepts a reference to the object acting as the command line manager, i.e. the object tagged with the various command line attributes and the object that will store the values of the command line arguments. It also optionally accepts a NumberFormatInfo parameter that defines how any numerical values will be parsed. Specifying this is recommended, since otherwise you may not know how numbers will be parsed. For an example I live in Sweden, and we use the comma (,) character as the decimal separator and not the dot (.) normally used in programming languages, so if I don't specify the number format it expects me to enter decimal values with the comma as the decimal separator, something I'm not used to doing in a program that is otherwise not localized for Sweden.

The constructor will parse the object passed to it for any command line attributes applied to it and verify the consistency of the parameters specified and the members to which the attributes are applied. If an error is detected an exception of type AttributeException will be thrown. This exception is derived from LogicException which indicates a programming error as opposed to a usage error. Such an exception should never be thrown when you are finished with your program.

5.2 Parsing the command line

The actual parsing is done by the Parse() method of the command line parser. There are several overloads of this method accepting various inputs containing the command line to parse. This may be a single string, an array of strings or a TextReader. The default Parse() method that doesn't accept any parameters uses the command line found in Environment.CommandLine as its input. These methods also accepts an additional parameter, containsExecutable which indicates whether the first argument on the command line is actually the path to the executable file that was run. This is normally the case, but should you pass it an input that does not contain this you should set this to false. If nothing is specified it assumes that the executable is present on the command line. If the executable file is present on the command line it may later be retrieved from the ExecutablePath property of the command line parser. This property will be a null reference if no executable was present.

5.3 Error handling

When parsing is complete, you can check the HasErrors property of the command line parser which indicates whether any errors were found during parsing. This includes any custom validation errors indicated by any setters of the command line manager object that threw a InvalidOptionValueException when the parser attempted to set a value. The actual errors are represented as objects of type ErrorInfo and can be retrieved from the Errors property which is a read-only collection. Errors can also be printed from the usage information object available from the UsageInfo property (see Section 5.4).

If no errors were present, all options processed have triggered a setter in the command line manager which should now have all its members set to values reflecting the options passed on the command line. Any remaining arguments from the command line, i.e. those that were not options or values for options can be retrieved from the collection RemainingArguments of the command line parser. This is typically input files or something similar, but may be whatever you like. You need to verify these manually since it is not done automatically by the command line parser in any way.

The ErrorInfo class contains an enumerated value indicating the type of the error and an error message describing the error. It may also contain the file name and line number of the file from which the options were parsed in case a list file or option file was specified on the command line and the OptionStyles.File flag was enabled in the parser. See the API documentation for more information on this.

5.4 Generating Usage Information

The property UsageInfo of the command line parser returns an object of type UsageInfo. This object contains the descriptive information of all the command line attributes applied to the command line manager, and can be used for retrieving formatted strings containing this information. It can also be used for setting this information programmatically, which is useful for internationalization (see Section 5.5). It contains the following methods which may be used for retrieving formatted strings:

GetHeaderAsString 
This method returns a string formatted in a field of the specified width containing a typical program header consisting of the application name, its version and copyright notice. This is usually the first thing output by any console program.
GetOptionsAsString 
This method returns a string containing a list of all options available to the program, formatted into two columns (whose widths may be individually specified). The options are grouped according to their groups and the descriptive information from each option group is also included.
GetErrorsAsString 
This method returns a formatted list of all the errors available from the Errors property of the command line parser used to create this instance.
ToString 
This method returns a string which is a concatenation of the strings returned by the three methods above. The error information can be omitted if so desired (such as when reacting to the help option of a program) by specifying a boolean parameter to this method.
The options and groups are also exposed individually using the classes OptionInfo and OptionGroupInfo which also expose the descriptive information for these items, both for setting and retrieving formatted strings. See the API documentation for more details on this.

5.5 Internationalization (i18n)

The Plossum library was designed with internationalization of messages in mind. Currently it only contains strings for the en-US locale, but hopefully more will be added in time. (If you want to translate these strings, please let me know, as I can only translate them to Swedish). However, while specifying descriptions and names in the attributes is simple, easy to manage and understand, it leaves something to be desired in terms of internationalization. As far as I know there is no way to set the parameter of an attribute to a string defined in a resource file, which makes internationalization of descriptions and such a problem. To solve this problem these descriptive properties need to be set programmatically, and this can be done in the UsageInfo object, described in Section 5.4. The API documentation provides a list of the available properties for setting descriptive information through this object.

All the strings of the Plossum library are formatted using the culture information found in CultureInfo.CurrentUICulture.

6 Advanced customization

There is still some more customization that can be made to the parser, such as what assignment characters to use for different option styles, the quotation marks that may be used for quoting values, and the characters that can be escaped within those quotations. The default settings for these options should be appropriate for most applications, but they are there for your customization if you need it.

6.1 Specifying assignment characters

The three methods AddAssignmentCharacter, RemoveAssignmentCharacter and ClearAssignmentCharacters of the CommandLineParser class provide you with a way of specifying which characters can be used for assigning a value to an option. Together with the assignment character you also specify for which option styles this assignment character is recognized. By default the available assignment characters are the equal sign (=) for any option accepting a value, and for Windows style options the colon (:) can also be used. If you want to completely change this behavior you should probably call ClearAssignmentCharacters before adding the styles you want to provide to remove any existing default definitions.

Note that the space delimiter is always available for use as assignment unless RequireExplicitAssignment is set to true for the option.

6.2 Specifying quotation marks and escape sequences

To be able to specify values containing for an example a white space, the command line parser has to provide some way of quoting values or escaping single characters. By default the double quotes (") are the only recognized quotation mark. Inside the quotes you may escape the quote-character itself to insert a literal quotation mark into the value, and you may also escape the escape character itself, the backslash (\). For an example: "ex1.exe -name="This is a \"quoted\" value"" which would assign the value This is a "quoted" value to the name option. Escaping characters are also available for unquoted values. The characters that can be escaped there are only the space character and the escape character itself, so you could write e.g. "ex1.exe -name=Peter\ Palotas" and the name Peter Palotas would be assigned to the name option. This is the default behavior, and is again suitable for most applications, but should you have other requirements you can change this.

The three methods AddQuotation, RemoveQuotation and ClearQuotations let you specify your own quotation rules in addition to, or instead of the default ones provided. AddQuotation accepts a single argument of the type QuotationInfo. This object contains a property QuotationMark of type char which represents the quotation character to be used. The special value of the null character ('\0') indicates that the escape sequences specified apply to any unquoted values. The QuotationInfo object also contain methods for specifying escape sequences (of one character length). This allows you to specify a character that can be escaped, and with what character it should be replaced.

Listing 6.1: Example of setting quotations

```
// Create a quotation for the single quote character (')
QuotationInfo qinfo = new QuotationInfo('\'');

// Let us escape the single quote
qinfo.AddEscapeCode('\'', '\'');

// Also let us escape the double quote
qinfo.AddEscapeCode('\"', '\"');

// And allow us to escape the escape character
qinfo.AddEscapeCode('\\', '\\');

// And finally the newline character (represented by 'n')
qinfo.AddEscapeCode('n', '\n');

// Now let us add this to the parser
parser.AddQuotation(qinfo);
```

The example above illustrates how this works by introducing single quotes as a valid quotation method to the parser, and also adding the newline escape sequence.

6.3 Specifying the escape character

The escape character itself can also be changed from its default which is the backslash (\) character. Actually multiple allowed escape characters may be specified. The backslash however is the standard pretty much about everywhere that I know about, but I have heard rumors claiming that some OS used the '^' character for escaping. The option is there if you want it anyway, and this is done by the SetEscapeCharacters() method of the CommandLineParser which accepts an enumeration containing the valid escape characters.

7 Implementation details (in short)

The Plossum command line library obviously uses reflection heavily to simplify the usage by the use of attributes. A lot of error checking for sanity of the placement of the attributes and its parameters are done in the CommandLineParser constructor. The parser is handwritten and the actual parsing is not that complicated, even though the error checking and handling of all the options makes for quite a bit of code in the Parse method. The parser repeatedly calls the getNextToken() of the lexer, which is also written by hand. The task of the lexer is to parse the characters of the command line turning them into so called Tokens. The following tokens exist and are returned by the lexer.

OptionNameToken Represents the name of an option and contains information about how the option was introduced
AssignmentToken 
Represents one of the available assignment characters
ValueToken 
Represents a value assigned to an option, or one of the remaining arguments. Quotes are stripped and escape characters escaped by the lexer.
FileToken 
Represents a list file or option file as specified on the command line.
The parser actually has a stack of lexers, so that when a FileToken is read, it can create a new lexer using the file as input pushing that on the stack making it the "current lexer", and continue retrieving tokens from the lexer on the top of the stack. When there are no more tokens from a lexer, it simply pops the lexer from the stack and continues reading from the new lexer on the top of the stack until the stack is empty which is when the parsing is complete.

Formatting of strings are done through the Plossum.StringFormatter class which contain a bunch of static methods that word wrap strings and format them into columns. The API documentation has more details on this.

8 Final words

It is my feeling that this library provides a very customizable, yet simple to use set of classes for specifying and parsing command line arguments. That was the goal of developing this library anyway, and I feel that I have succeeded.

The library is as of the time this tutorial was written not extensively tested and bound to contain several bugs. Please help me (and yourself) by reporting any bugs found or by submitting a patch to them.

The Plossum library actually contains a lot more than just the command line parser, though that functionality was stripped out for the version found on this web page. The Plossum library is an open source project hosted at SourceForge.net and can be found here. Please submit any bugs or feature requests using the tracker on that page. The most up to date version of this library can always be found there.

If you want to contribute to this library, for example by fixing bugs, adding features, or much appreciated, translating strings, please drop a note in one of the forums on SourceForge. If you have other questions, post them there on the forum as well.

I hope you find this library useful, and don't hesitate to ask questions on the forums or submit bug reports.
