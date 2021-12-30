using LehmanLaidun.FileSystem;
using System.Management.Automation;
using System.Xml;

namespace LehmanLaidun.Powershell;

[Cmdlet("Get", "Structure")]
public class GetStructure : PSCmdlet
{
    //[Option("mypath", Required = true, HelpText = "A path to compare with another.")]
    [Parameter(Mandatory = true, Position = 0)]
    public string MyPath { get; set; } = "";

    protected override void BeginProcessing()
    {
        base.BeginProcessing();
    }

    protected override void ProcessRecord()
    {
        base.ProcessRecord();
        WriteVerbose("ProcessRecord.Begin, MayPath:" + MyPath);

        var files = LogicFactory.CreateForPath(
            new System.IO.Abstractions.FileSystem(),
            MyPath,
            PluginHandler.Create());

        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(files.AsXDocument().ToString());

        WriteVerbose("Files.Begin");
        //WriteObject(files.AsXDocument());
        WriteObject(xmlDocument);
        WriteVerbose("Files.End.");

        WriteVerbose("ProcessRecord.End");
    }

    protected override void EndProcessing()
    {
        base.EndProcessing();
    }

}
