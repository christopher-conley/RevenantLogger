# TODO:

## General

- [ ] Add cmdlet (`Read/Get/Watch-RevenantLogFile`) to render a logfile to the console with original markup.
  - [ ] Will need to figure out a way to render the markup without having Spectre.Console choke and die on
        log messages that may contain its own markup characters, like a PowerShell reference to a type: [System.IO.File]. 
        Maybe save the marked-up logfile as a separate file and `.Replace()` those characters with their respective
        character codes and then do the reverse on read-in, or something like that.

  - [ ] Add an alias (`List-RevenantLogFile`) or parameter (`-List`) to enumerate logfiles in the directory specified by the `-Path` setting, or
        the default log directory if not specified.

  - [ ] Add a parameter (`-Path`/`-File`) to specify the path to the logfile.
  - [ ] Add a parameter (`-Line`) to specify the line number to start rendering from.
  - [ ] Add a parameter (`-Count`) to specify the number of lines to render.
  - [ ] Add a parameter (`-Tail`) to specify that the cmdlet should render the last X lines of the file.
  - [ ] Add a parameter (`-Follow`) to continuously poll the file for new lines and render them, same parameterset as `-Tail`.
  - [ ] Add a parameter (`-Head`) to specify that the cmdlet should render the first X lines of the file.
  - [ ] Add a parameter (`-Highlight`) to specify a string to highlight in the rendered output, a poor man's grep.
  - [ ] Add a parameter (`-CaseSensitive`) to specify that the `-Highlight` string should be case-sensitive, same parameterset as `-Highlight`.
  - [ ] Add a parameter (`-Regex`) to specify that the `-Highlight` string should be treated as a regex pattern, same parameterset as `-Highlight`.
  - [ ] Add a parameter (`-Color`) to specify the color of the highlighted text, same parameterset as `-Highlight`.
  - [ ] Add a parameter (`-Reverse`) to specify that `-Color` should be the background color of the highlighted text instead of foreground.
  - [ ] Add a parameter (`-NoColor/-NoMarkup`) to specify that the output should be rendered without markup.
  - [ ] Add a parameter (`-LineNumbers`) to specify that the output should include line numbers.
  - [ ] Add a parameter (`-NoTimestamps`) to specify that the output should not include timestamps.
  - [ ] Add a parameter (`-NoLoggers`) to specify that the output should not include logger names.
  - [ ] Add a parameter (`-NoLevels`) to specify that the output should not include log/severity levels.
  - [ ] Add a parameter (`-Playback/-Realtime`) to specify that the output should be rendered at the same rate as
        the logfile was written (read timestamps from the file and use stopwatch to wait?).

- [ ] Add cmdlet (`Get-RevenantLogFileStats`) to get useless stats about a logfile.
  - [ ] Add a parameter (`-Path`/`-File`) to specify the path to the logfile.
  - [ ] Add a parameter (`-LineCount`) to specify that the cmdlet should return the number of lines in the file.
  - [ ] Add a parameter (`-WordCount`) to specify that the cmdlet should return the number of words in the file (whitespace delimited?).
  - [ ] Add a parameter (`-CharCount`) to specify that the cmdlet should return the number of characters in the file.
  - [ ] Add a parameter (`-ByteCount`) to specify that the cmdlet should return the number of bytes in the file.
  - [ ] Add a parameter (`-Size`) to specify that the cmdlet should return the size of the file in bytes.
  - [ ] Add a parameter (`-Loggers`) to specify that the cmdlet should return the loggers in the file, and the most used.
  - [ ] Add a parameter (`-Levels`) to specify that the cmdlet should return the log/severity levels in the file, and the most used.
  - [ ] Add a parameter (`-Callers`) to specify that the cmdlet should return the number of unique callers in the file, and the most used.
  - [ ] Add a parameter (`-SessionLength`) to specify that the cmdlet should return the length of the log session, determined by the first and last timestamp.
  - [ ] Add a parameter (`-All`) to specify that the cmdlet should return all useless shit available to it.

- [ ] Add cmdlet (`Export-RevenantLogger`) to export a logger & its settings to a CLIxml or JSON file.
  - [ ] Add a parameter (`-Path`/`-File`) to specify the path to save the exported logger file to.
  - [ ] Add a parameter (`-All`) to specify all loggers should be exported.

- [ ] Add cmdlet (`Import-RevenantLogger`) to import a logger & its settings from a CLIxml or JSON file (call CmdNewRevenantLogger to create).
  - [ ] Add a parameter (`-Path`/`-File`) to specify the path to load the exported logger file from.
  - [ ] Add a parameter (`-All`) to specify all loggers in the file should be created.
  - [ ] Add a parameter (`-Name`) to specify a specific logger by name
  - [ ] Add a parameter (`-GUID`) to specify a specific logger by GUID
  - [ ] Skip default logger creation in the DI container if the `-All` parameter is specified and use the imported
        one instead.

- [ ] Make the `-Config` parameters to cmdlets actually do something

- [ ] Turn on XML autodoc generation
  - [ ] Use [platyPS](https://github.com/PowerShell/platyPS) or similar to create the Cmdlet help from Markdown documentation
        so I don't actively want to die while I'm doing it.

- [ ] Create Github Actions workflow to automatically build and publish the module to PSGallery & Github Releases
  - [ ] Add MFA seed for code-signing cert to Github Secrets and generate TOTP code from that, and find out how to
        actually sign it because Certum only makes a Desktop version of their app apparently.

- [ ] Change any internal error messages to use `WriteError()` and `throw` instead of pushing the messages through the logger's
      `.Log()` methods so they can be caught by user error handling and so we're not mucking up the logfile with internal bullshit.
  - [ ] Make this an option in the config file, a common parameter to cmdlets, or both. Have "on/off/both" options, default to "Off/WriteError()".

- [ ] See if Claude Sonnet can generate some Pester tests because I don't wanna do it.

## ConfigDefinition.cs

- [ ] Add an `Editor` config option to specify a path to the preferred text editor for module config files

## DependencyInjection.cs

- [ ] Save the DI container as a PowerShell variable so it can be passed between runspaces/sessions

## Get-RevenantLoggerConfig.cs

- [ ] Add a parameter (`-Default`) to render a default config (string) from a new `Configuration()` object.
- [ ] Add a parameter (`-Logger`) to render the config for a specific logger.
- [ ] Add a parameter (`-Pretty`) to render the config as pretty JSON text.
- [ ] Add a parameter (`-Saveconfig`) to save a config file based on the logger settings (common parameter with `Get-RevenantLogger`).
  - [ ] Add a parameter (`-Path`) to specify the path to save the config file to.
  - [ ] Add a parameter (`-Name`) to specify the name of the config file.
  - [ ] Add a parameter (`-Overwrite`) to specify that the config file should be overwritten if it already exists.

## Edit-RevenantLoggerConfig.cs

- [ ] Prefer the `-Editor` parameter to the cmdlet first, then `Editor` config file option, then manually search paths
- [ ] Add a parameter (`-NoBackup`) to specify that the config file should not be backed up before changes are made.
- [ ] Add a parameter (`-Reset`) to reset the config file to the default settings.
- [ ] Add a parameter (`-Get`) to get the value of a specific setting in the config file.
- [ ] Add a parameter (`-Set`) to set the value of a specific setting in the config file.
- [ ] Add a parameter (`-Remove`) to remove a specific setting from the config file.

## Get-RevenantLogger.cs

- [ ] Add a parameter (`-Logger`) to allow the user to specify the logger they want to retrieve.
  - [ ] Should accept a string or an array of strings, where the string is the Name or GUID of the logger.
    -  Need to change `_iloggersList` and `ILoggersList` in RevenantLoggerBase.cs to match the Type of
       the `_userCustomLoggers` var or a Type that can be cast to it to accommodate this. Or just scrap the
       existing `AddToLoggersList()` method and replace it with the existing `AddToCustomLoggers()` method and rename it.

- [ ] Add a parameter (`-New`) to create a new logger which calls the `CmdNewRevenantLogger` cmdlet.
  - [ ] Add the relevant existing `CmdNewRevenantLogger` parameters to this cmdlet in their own ParameterSet with `-New`

- [ ] Add a parameter (`-Saveconfig`) to save a config file based on the logger settings (common parameter with `Get-RevenantLoggerConfig`).
  - [ ] Add a parameter (`-Path`) to specify the path to save the config file to.
  - [ ] Add a parameter (`-Name`) to specify the name of the config file.
  - [ ] Add a parameter (`-Overwrite`) to specify that the config file should be overwritten if it already exists.

## ILoggerExtensions.cs

- [ ] - [ ] Make the caller and and `()` colors configurable in the config file

## Write-RevenantLog.cs

- [ ] Add a parameter (`-Logger`) to allow the user to specify the logger to use when writing the log message.
  - [ ] Should accept a raw `ILogger` object.
  - [ ] Should accept a raw `UserLogger` object.
  - [ ] Should accept a raw `UserCustomLogger` object.
  - [ ] Should accept a string or an array of strings, where the string is the Name or GUID of the logger.

## TypeFormatters.cs

- [ ] Actually use the shit that I put in it

## ValidateType.cs

- [ ] Make it not shitty

## LoggingStyleTypes.cs

- [ ] Change any hardcoded markup tags to be configurable options in the config file

## RevenantLoggerBase.cs

- [ ] General cleanup, move some methods/properties to `RevenantLoggerPSCmdlet` class if they make more sense there

## RevenantLoggerPSCmdlet.cs

- [ ] Move the `GetFlattenedArray()`-related stuff into its own file and/or the `Utilities` class?

## StaticStrings.cs

- [ ] Move more static strings to this file

## UserLogger.cs

~~- [x] Add the missing `.Log()` convenience methods like `.LogInformation()` etc. so a loglevel doesn't have to be
      specified with each manual call a user might make with a raw logger object~~

## Logging.cs

- [ ] Make it not shitty
- [ ] Make it less dependent upon the config so it's easier to implement multiple loggers with independent configs

## Utilities.cs

- [ ] General cleanup
