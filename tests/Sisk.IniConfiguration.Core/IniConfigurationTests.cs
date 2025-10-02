using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sisk.IniConfiguration.Core.Tests;

[TestClass]
public class IniDocumentTests
{
    [TestMethod]
    public void TestFromString_ParsesSimpleIniFile()
    {
        string iniContent = """
            key1=value1
            key2 = value2
            """;

        var iniDocument = IniDocument.FromString(iniContent);
        Assert.AreEqual("value1", iniDocument.Global.GetOne("key1"));
        Assert.AreEqual("value2", iniDocument.Global.GetOne("key2"));
    }

    [TestMethod]
    public void TestFromString_ParsesIniFileWithSections()
    {
        string iniContent = """
            [Section1]
            key1=value1

            [Section2]
            key2=value2
            """;

        var iniDocument = IniDocument.FromString(iniContent);
        var section1 = iniDocument.GetSection("Section1");
        var section2 = iniDocument.GetSection("Section2");

        Assert.IsNotNull(section1);
        Assert.IsNotNull(section2);
        Assert.AreEqual("value1", section1["key1"][0]);
        Assert.AreEqual("value2", section2["key2"][0]);
    }

    [TestMethod]
    public void TestFromString_ParsesIniFileWithComments()
    {
        string iniContent = """
            ; comment
            key1=value1 # another comment
            """;

        var iniDocument = IniDocument.FromString(iniContent);
        Assert.AreEqual("value1", iniDocument.Global["key1"][0]);
    }

    [TestMethod]
    public void TestFromString_ParsesIniFileWithCommentsInStrings()
    {
        string iniContent = """
            ; comment
            key1="value1 # another comment" # actual comment
            key2 = 'value
            with ; hash' ; comment
            """;

        var iniDocument = IniDocument.FromString(iniContent);
        Assert.AreEqual("value1 # another comment", iniDocument.Global["key1"][0]);
        Assert.AreEqual("value\r\nwith ; hash", iniDocument.Global["key2"][0]);
    }

    [TestMethod]
    public void TestFromString_ParsesIniFileWithMixedStrings()
    {
        string iniContent = """
            key3 = '"mixed"'
            key4 = "'mixed'"
            """;

        var iniDocument = IniDocument.FromString(iniContent);
        Assert.AreEqual("\"mixed\"", iniDocument.Global["key3"][0]);
        Assert.AreEqual("'mixed'", iniDocument.Global["key4"][0]);
    }

    [TestMethod]
    public void TestFromString_ParsesIniFileWithExplodedString()
    {
        string iniContent = """
            key = "foo, hello
            """;

        var iniDocument = IniDocument.FromString(iniContent);
        Assert.AreEqual("foo, hello", iniDocument.Global["key"][0]);
    }

    [TestMethod]
    public void TestFromString_ParsesIniFileWithDuplicateKeys()
    {
        string iniContent = """
            key1=value1
            key1=value2
            """;

        var iniDocument = IniDocument.FromString(iniContent);
        var values = iniDocument.Global["key1"];
        Assert.AreEqual(2, values.Length);
        Assert.AreEqual("value1", values[0]);
        Assert.AreEqual("value2", values[1]);
    }

    [TestMethod]
    public void TestFromString_ParsesIniFileWithMultiLineValues()
    {
        string iniContent = """
            key1='line1
            line2'
            key2="line3
            line4"
            key3=<<<EOF
            line3
            value = with equals
            EOF
            key4=<<<Another-HereDoc-Start
            <<< complicated value
            Another-HereDoc-Start
            """;
        var iniDocument = IniDocument.FromString(iniContent);
        Assert.AreEqual("line1\r\nline2", iniDocument.Global["key1"][0]);
        Assert.AreEqual("line3\r\nline4", iniDocument.Global["key2"][0]);
        Assert.AreEqual("line3\r\nvalue = with equals", iniDocument.Global["key3"][0]);
        Assert.AreEqual("<<< complicated value", iniDocument.Global["key4"][0]);
    }

    [TestMethod]
    public void TestToString_WritesIniDocument()
    {
        var iniDocument = new IniDocument();
        iniDocument.Global.Add("key1", "value1");
        var section1 = new IniSection("Section1");
        section1.Add("key2", "value2");
        iniDocument.Sections.Add(section1);

        string expectedIni = """
            key1 = value1

            [Section1]
            key2 = value2

            """;

        Assert.AreEqual(expectedIni.Trim().ReplaceLineEndings(), iniDocument.ToString().Trim().ReplaceLineEndings());
    }

    [TestMethod]
    public void TestGetEntry_ReturnsCorrectValue()
    {
        var iniDocument = new IniDocument();
        iniDocument.Global.Add("key1", "value1");
        var section1 = new IniSection("Section1");
        section1.Add("key2", "value2");
        iniDocument.Sections.Add(section1);

        var value1 = iniDocument.GetEntry("key1");
        var value2 = iniDocument.GetEntry("Section1.key2");

        Assert.AreEqual(1, value1.Length);
        Assert.AreEqual("value1", value1[0]);
        Assert.AreEqual(1, value2.Length);
        Assert.AreEqual("value2", value2[0]);
    }

    [TestMethod]
    public void TestGetSection_ReturnsCorrectSection()
    {
        var iniDocument = new IniDocument();
        var section1 = new IniSection("Section1");
        iniDocument.Sections.Add(section1);

        var retrievedSection = iniDocument.GetSection("Section1");
        Assert.IsNotNull(retrievedSection);
        Assert.AreEqual("Section1", retrievedSection.Name);
    }

    [TestMethod]
    public void TestGlobalSection_CanAddAndGetValues()
    {
        var iniDocument = new IniDocument();
        iniDocument.Global.Add("key", "value");
        Assert.AreEqual("value", iniDocument.Global["key"][0]);
    }
}

[TestClass]
public class IniSectionTests
{
    [TestMethod]
    public void TestGetOne_ReturnsLastValue()
    {
        var section = new IniSection("TestSection");
        section.Add("key", "value1");
        section.Add("key", "value2");
        Assert.AreEqual("value2", section.GetOne("key"));
    }

    [TestMethod]
    public void TestGetMany_ReturnsAllValues()
    {
        var section = new IniSection("TestSection");
        section.Add("key", "value1");
        section.Add("key", "value2");
        var values = section.GetMany("key");
        Assert.AreEqual(2, values.Length);
        Assert.AreEqual("value1", values[0]);
        Assert.AreEqual("value2", values[1]);
    }

    [TestMethod]
    public void TestAdd_AddsValue()
    {
        var section = new IniSection("TestSection");
        section.Add("key", "value");
        Assert.AreEqual("value", section["key"][0]);
    }

    [TestMethod]
    public void TestRemove_RemovesValue()
    {
        var section = new IniSection("TestSection");
        section.Add("key", "value");
        section.Remove("key");
        Assert.IsFalse(section.ContainsKey("key"));
    }
}