/* XmlFile writer
 * ==============
 * 
 * The XmlFlie utility allows for reading and writing a xml file based
 * on a xml class object.
 * 

 * Author: Josh Holzworth
 * Last Modified: 2/14/2018
 */

using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace FILEHANDLER.XmlFile
{
    /// <summary>
    /// 
    /// </summary>
    public class XmlFileException : Exception
    {
        public XmlFileException()
            : base()
        {
        }

        public XmlFileException(string msg)
            : base(msg)
        {
        }

        public XmlFileException(string msg, Exception innerException)
            : base(msg, innerException)
        {
        }
    }

    /// <summary>
    /// XmlFile control
    /// 
    /// This class create the read and write ability of a xml file based
    /// on a xml class used in the initialization
    /// 
    /// Example:
    /// private XmlFile<DataFile> _xmlFile = new XmlFile<DaraFile>();
    /// </summary>
    /// <typeparam name="T">class object defining xml structure</typeparam>
    public class XmlFile<T>
        where T : class
    {
        public XmlFile() { }

        public XmlFile(out T obj, string file)
        {
            obj = ReadFile(file);
        }

        public XmlFile(out T obj, Stream file)
        {
            obj = Read(file);
        }

        /// <summary>
        /// Handles an undefined node in the xml file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void _SerializerUnknownNode(object sender, XmlNodeEventArgs e)
        {
            throw new XmlFileException(
                String.Format("Unknown node name:[{0}] value:[{1}]", e.Name, e.Text));
        }

        /// <summary>
        /// Handles an undefined attribute in the Xml file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="">RecipeFileException</exception>
        private static void _SerializerUnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            System.Xml.XmlAttribute attr = e.Attr;
            throw new XmlFileException(
                String.Format("Unknown attribute attr:[{0}] value:[{1}]", attr.Name, attr.Value));
        }

        public string Write(T xmlObject)
        {
            if (xmlObject == null) return string.Empty;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                StringWriter sw = new StringWriter();
                using (XmlTextWriter writer = new XmlTextWriter(sw))
                {
                    writer.Formatting = Formatting.Indented;
                    serializer.Serialize(writer, xmlObject);
                    return sw.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new XmlFileException("Write error", ex);
            }
        }

        public T Read(Stream file)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                serializer.UnknownNode += new XmlNodeEventHandler(_SerializerUnknownNode);
                serializer.UnknownAttribute += new XmlAttributeEventHandler(_SerializerUnknownAttribute);

                T xmlObject = (T)Activator.CreateInstance(typeof(T));
                xmlObject = (T)serializer.Deserialize(file);

                if (xmlObject == null)
                    throw new FileLoadException(String.Format("File [{0}] not found", file));
                return xmlObject;
            }
            catch (Exception ex)
            {
                throw new XmlFileException("Read error", ex);
            }
        }

        public T ReadFile(string filePath)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                serializer.UnknownNode += new XmlNodeEventHandler(_SerializerUnknownNode);
                serializer.UnknownAttribute += new XmlAttributeEventHandler(_SerializerUnknownAttribute);

                FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                T xmlObject = (T)Activator.CreateInstance(typeof(T));
                xmlObject = (T)serializer.Deserialize(fs);

                fs.Close();
                fs = null;

                if (xmlObject == null)
                    throw new FileLoadException(String.Format("File [{0}] not found", filePath));
                return xmlObject;
            }
            catch (Exception ex)
            {
                throw new XmlFileException("Read file error", ex);
            }
        }
    }
}
