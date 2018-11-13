# Readme for LehmanLaidun

License is AGPL+NoEvil.

State of project is Alpha.

## Problem solved

Finding duplicate pictures.
Cleaning your hullerombuller backups.

You have a lot of pictures backed up on your harddrive. And then on another drive from your phone. And you spouse's images resides somehwere. Plus some stray vacation pictures. And the one of your band.  
You don't have them in order and they are backup up everywhere and lots of doubles; so you pay 50% to much for your backup plan.

This project aims to find the doubles and help you get one and only one of every picture in your backup.

### Positive side effect

The mechanism is the same so why not use it for aligning your and your friends' collections of music and movies?

Or at your company - everyone dumps everything everywhere and what is really the canonical reference? find the doubles and eliminate them.

## Road map

A GUI or console. Point it at two folders. Get a report showing differences.

A GUI or console. Point it at a folder. Get a report of duplicates and possible duplicates.  
It says "possible" duplicates since two files with the exact same content but different names might be or not be the same file. If it is vacation pictures it is probably the same file, unless one is a copy in the folder "Send to print shop".

Then we can add functionality for easing the diffing; like an image viewer to quickly choose the one to keep.
Configurability for, for example, if a file with the same content but different file name is different or not.

## Example of usage through code

### Get info files and folder info from a folder structure. Return all files as a list or as an xml tree.

    using LehmanLaidun.FileSystem;
    using System.Collections.Generic;
    using System.Xml.Linq;
    
    Logic logic = LehmanLaidun.FileSystem.LogicFactory.CreateForPath(@"D:\");
    IEnumerable<FileItem> files = logic.AsEnumerableFiles();
    XDocument xml = logic.AsXDocument();

### Compare 2 xml folder structures. Return a list of differences.

    var doc1 = LogicFactory.CreateForPath(@"C:\MyPictures").AsXDocument();
    var doc2 = LogicFactory.CreateForPath(@"D:\MyUsb").AsXDocument();
    
    (bool Result, IEnumerable<Difference> Differences) comparison = CompareXml( (XDocument) doc1, (XDocument) doc2 );

    Console.WriteLine( $"Folder structure is equal:{comparison.Result}."); // "Folder structure is equal:False".
    foreach( var diff in comparison.Differences ){
        Console.WriteLine( ... )
    }

There is a a skeleton of a console application that compare two folder structures.

### Find all duplicate files in a folder structure.

    var doc = XDocument.Parse(xmlString);
	var differences = Logic.FindDuplicates(doc);

	foreach( var diff in differences ){
		Console.WriteLine( ... )
	}

### Find all duplicates and all similar, "almost duplicates", elements in an xml.

	var doc = XDocument.Parse(xmlString);
	var similars = Logic.FindSimilars(
		doc, 
		new[]{
			Logic.Rule.Create(
				"Duplicate",
				(element1, element2) =>
					element1.Name == "File" && element2.Name == "File" &&
					element1.Attribute("Name").Value == element2.Attribute("Name").Value &&
						element1.Attribute("Size")?.Value == element2.Attribute("Size")?.Value;
				)
			)
			Logic.Rule.Create( 
				"Same size", 
				new Logic.Rule.CompareDelegate[]{
					(element1, element2) =>
						element1.Attribute("Size")?.Value == element2.Attribute("Size")?.Value;
				}
			}
		}
	);

	foreach( var similar in similars ){
		Console.WriteLine(...).
	}

	In the above example the Rules are created in different ways only because it is possible.