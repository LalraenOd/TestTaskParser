USE TestCaseDb
GO

IF OBJECT_ID('Parts') IS NOT NULL
	DROP TABLE Parts
GO

IF OBJECT_ID('LinkParts') IS NOT NULL
	DROP TABLE LinkParts
GO	

CREATE TABLE LinkParts(
Id int IDENTITY PRIMARY KEY NOT NULL,
PartNumber varchar(20)
);
GO

CREATE TABLE Parts(
Id int IDENTITY PRIMARY KEY NOT NULL,
URL varchar(70),
ArtNumber varchar(20),
BrandName nvarchar(20),
PartName nvarchar(30),
Specs xml,
LinkedParts int FOREIGN KEY REFERENCES LinkParts(Id)
);
GO
