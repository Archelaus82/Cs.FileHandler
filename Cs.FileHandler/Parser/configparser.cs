using System;
using System.Collections.Generic;
using System.IO;

namespace QC.ADDONS.FILEHANDLER
{
    public class NoSectionException : Exception
    {
        public NoSectionException(string message)
            : base(message)
        {
        }
    }

    public class DuplicateSectionException : Exception
    {
        public DuplicateSectionException(string message)
            : base(message)
        {
        }
    }


    /// <summary>
    /// Config file parser
    /// 
    /// This class is designed to replicate the native ConfigParser
    /// in Python.
    /// </summary>
    public class ConfigParser
    {
        const string IGNORE = "!";

        List<string> _sections;
        Dictionary<string, List<OptionValue>> _options;
        Dictionary<string, int> _sectionLineNumbers;
        Dictionary<string, Dictionary<string, int>> _optionLineNumbers;

        public struct OptionValue
        {
            string _option;
            string _value;

            public OptionValue(string option, string value)
            {
                _option = option;
                _value = value;
            }
            
            public string Option { get { return _option; } }
            public string Value { get { return _value; } }
        }

        public ConfigParser()
        {
            _sections = new List<string>();
            _options = new Dictionary<string, List<OptionValue>>();
            _sectionLineNumbers = new Dictionary<string, int>();
            _optionLineNumbers = new Dictionary<string, Dictionary<string, int>>();
        }

        /// <summary>
        /// Return the list of sections.
        /// </summary>
        /// <returns>list of sections</returns>
        public List<string> Sections { get { return _sections; } }

        /// <summary>
        /// Add a section.
        /// 
        /// If the section already exists, DuplicatSectionException is thrown.
        /// </summary>
        /// <param name="section">name of the section</param>
        public void AddSection(string section)
        {
            if (_sections.Contains(section))
                throw new DuplicateSectionException(String.Format("Section already exists [{0}]", section));

            _sections.Add(section);
            _options[section] = new List<OptionValue>();
        }

        /// <summary>
        /// Does the section exist.
        /// 
        /// If the section exists, return true; else, return false.
        /// </summary>
        /// <param name="section">name of section</param>
        /// <returns>true/false</returns>
        public bool HasSection(string section)
        {
            if (_sections.Contains(section))
                return true;

            return false;
        }

        /// <summary>
        /// Returns the list of options for the section.
        /// 
        /// If the section does not exist, NoSectionException is thrown.
        /// </summary>
        /// <param name="section">name of the section</param>
        /// <returns>list of options</returns>
        public List<string> Options(string section)
        {
            if (!HasSection(section))
                throw new NoSectionException(String.Format("Section invalid [{0}]", section));

            List<string> options = new List<string>();
            foreach (KeyValuePair<string, List<OptionValue>> kvp in _options)
                foreach (OptionValue ov in kvp.Value)
                    options.Add(ov.option);

            return options;
        }

        /// <summary>
        /// Does the option exist.
        /// 
        /// If the section does not exist, NoSectionException is thrown.
        /// if the option exists, return true; else, return false.
        /// </summary>
        /// <param name="section">name of the section</param>
        /// <param name="option">name of the option</param>
        /// <returns>true/false</returns>
        public bool HasOption(string section, string option)
        {
            if (!HasSection(section))
                throw new NoSectionException(String.Format("Section invalid [{0}]", section));

            foreach (OptionValue ov in _options[section])
                if (option == ov.option)
                    return true;

            return false;
        }

        /// <summary>
        /// Return a list of OptionValue objects.
        /// 
        /// If the section does not exist, NoSectionException is thrown.
        /// </summary>
        /// <param name="section">name of the section</param>
        /// <returns>list of OptionValue objects</returns>
        public List<OptionValue> Items(string section)
        {
            if (!HasSection(section))
                throw new NoSectionException(String.Format("Section invalid [{0}]", section));

            return _options[section];
        }

        /// <summary>
        /// Get the value for the option in the section.
        /// 
        /// Return null if the option does not exist
        /// </summary>
        /// <param name="section">name of the section</param>
        /// <param name="option">name of the option</param>
        /// <returns>option value</returns>
        public string Get(string section, string option)
        {
            if (!HasOption(section, option))
                return null;

            foreach (OptionValue ov in Items(section))
                if (option == ov.option)
                    return ov.value;

            return null;
        }

        /// <summary>
        /// Add new option.
        /// 
        /// If the section does not exist, NoSectionException is thrown.
        /// </summary>
        /// <param name="section">name of the section</param>
        /// <param name="option">name of the option</param>
        /// <param name="value">value of the option</param>
        public void Set(string section, string option, string value = "")
        {
            if (!HasSection(section))
                throw new NoSectionException(String.Format("Section invalid [{0}]", section));

            _options[section].Add(new OptionValue(option, value));
        }

        /// <summary>
        /// Read the entries in the file
        /// </summary>
        /// <param name="file">File to process</param>
        /// <returns>True/False</returns>
        public bool Read(string file)
        {
            if (!File.Exists(file))
                return false;

            string[] lines = File.ReadAllLines(file);

            string lastSection = null;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(IGNORE))
                    continue;

                bool newSection = false;
                lines[i] = lines[i].TrimEnd('\n');
                if (lines[i].Contains("[") && lines[i].Contains("]"))
                    newSection = true;

                if (newSection)
                {
                    string section = lines[i].Substring(lines[i].IndexOf("[") + 1, lines[i].IndexOf("]") - (lines[i].IndexOf("[") + 1));
                    AddSection(section);
                    _sectionLineNumbers[section] = i;
                    _optionLineNumbers[section] = new Dictionary<string, int>();
                    lastSection = section;
                }
                else if (!String.IsNullOrEmpty(lines[i]) && !String.IsNullOrWhiteSpace(lines[i]))
                {
                    string option = null;
                    string value = null;
                    if (lines[i].Contains("="))
                    {
                        int equalIndex = lines[i].IndexOf("=");
                        option = lines[i].Substring(0, equalIndex).Trim();
                        value = lines[i].Substring(equalIndex + 1, lines[i].Length - (equalIndex + 1)).Trim();
                    }
                    else
                    {
                        option = lines[i].Trim();
                    }

                    Set(lastSection, option, value);
                    _optionLineNumbers[lastSection][option] = i;
                }
            }

            return true;
        }

        /// <summary>
        /// Write the contents of the parser to file.
        /// </summary>
        /// <param name="directory">file directory</param>
        /// <param name="filename">file name</param>
        public void Write(string directory, string filename)
        {
            string fileLocation = Path.Combine(directory, filename);

            try
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (StreamWriter sw = new StreamWriter(fileLocation))
                {
                    foreach (string section in _sections)
                    {
                        sw.WriteLine(String.Format("[{0}]", section));
                        foreach (OptionValue ov in Items(section))
                        {
                            if (String.IsNullOrEmpty(ov.value))
                                sw.WriteLine(ov.option);
                            else
                                sw.WriteLine(String.Format("{0} = {1}", ov.option, ov.value));
                        }

                        // add empty line after the section options
                        sw.WriteLine("");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Failed to write file [{0}]", fileLocation), ex);
                throw ex;
            }
        }

        /// <summary>
        /// Read in a config file
        /// </summary>
        /// <param name="directory">file directory</param>
        /// <param name="fileName">file name</param>
        public void Read(string directory, string fileName)
        {
            string fileLocation = Path.Combine(directory, fileName);
            string[] fileContent = File.ReadAllLines(fileLocation);
            List<OptionValue> _optionValues = new List<OptionValue>();
            string[] optionValuePair;
            OptionValue optionValue;
            _options = new Dictionary<string, List<OptionValue>>();
            _sections = new List<string>();

            foreach (string line in fileContent)
            {
                if (line.Contains("="))
                {
                    string lineSection = line.Split('=')[0].Trim();
                    _sections.Add(lineSection);

                    string lineContent = line.Split('=')[1];
                    lineContent = lineContent.Trim();
                    foreach (string options in lineContent.Split(' '))
                    {
                        optionValuePair = options.Split('|');
                        optionValue = new OptionValue(optionValuePair[0].Trim(), optionValuePair[1].Trim());
                        _optionValues.Add(optionValue);
                    }
                    _options.Add(lineSection, _optionValues);
                    _optionValues = new List<OptionValue>();
                }
            }

        }
    }
}
