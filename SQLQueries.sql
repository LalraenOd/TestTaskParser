USE TestCaseDb
GO

IF OBJECT_ID('LinkParts') IS NOT NULL
	DROP TABLE LinkParts
GO	

CREATE TABLE LinkParts(
Id int IDENTITY PRIMARY KEY NOT NULL,
PartNumber varchar(20),
PartBrandName varchar(20)
);
GO

IF OBJECT_ID('Parts') IS NOT NULL
	DROP TABLE Parts
GO

CREATE TABLE Parts
(
Id int IDENTITY PRIMARY KEY NOT NULL,
URL varchar(70) NOT NULL,
BrandName nvarchar(20) NOT NULL,
ArtNumber varchar(20) NOT NULL,
PartName nvarchar(30) NOT NULL,
--Specs xml,
Specs nvarchar(1000) NOT NULL,
LinkedParts int FOREIGN KEY REFERENCES LinkParts(Id)
);
GO

/****** Script for SelectTopNRows command from SSMS  ******/
SELECT TOP (1000) [Id]
      ,[URL]
      ,[BrandName]
      ,[ArtNumber]
      ,[PartName]
      ,[Specs]
      ,[LinkedParts]
  FROM [TestCaseDb].[dbo].[Parts]
  GO


--INSERT INTO dbo.Parts(URL, ArtNumber, BrandName,PartName, Specs) VALUES ()
CREATE PROCEDURE [dbo].[sp_InsertPart]
	@URL varchar(70),
	@BrandName nvarchar(20),
	@ArtNumber varchar(20),
	@PartName nvarchar(30),
	--@Specs xml,
	@Specs nvarchar(1000)
AS
	INSERT INTO Parts(URL, BrandName, ArtNumber, PartName, Specs)
	VALUES (@URL, @BrandName, @ArtNumber, @ArtNumber, @Specs)
GO

CREATE PROCEDURE [dbo].[sp_GetAllParts]
AS
	SELECT * FROM Parts
GO

CREATE PROCEDURE [dbo].[sp_GetPartsByNumber]
	@ArtNumberToFind varchar(20)
AS
	SELECT * FROM Parts
	WHERE ArtNumber = @ArtNumberToFind
GO