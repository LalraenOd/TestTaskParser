USE TestCaseDb
GO

IF OBJECT_ID('LinkParts') IS NOT NULL
	DROP TABLE LinkParts
GO	

CREATE TABLE LinkParts(
Id int IDENTITY PRIMARY KEY NOT NULL,
PartNumber varchar(100),
PartBrandName varchar(100)
);
GO

IF OBJECT_ID('Parts') IS NOT NULL
	DROP TABLE Parts
GO

CREATE TABLE Parts
(
Id int IDENTITY PRIMARY KEY NOT NULL,
URL varchar(100) NOT NULL,
BrandName nvarchar(100) NOT NULL,
ArtNumber varchar(100) NOT NULL,
PartName nvarchar(100) NOT NULL,
Specs nvarchar(1000) NOT NULL,
--LinkedParts int FOREIGN KEY REFERENCES LinkParts(Id)
);
GO

/****** Script for SelectTopNRows command from SSMS  ******/
SELECT TOP (1000) [Id]
      ,[URL]
      ,[BrandName]
      ,[ArtNumber]
      ,[PartName]
      ,[Specs]
  FROM [TestCaseDb].[dbo].[Parts]
  GO


DROP PROCEDURE [dbo].[sp_InsertPart]
GO

CREATE PROCEDURE [dbo].[sp_InsertPart]
	@URL varchar(100),
	@BrandName nvarchar(100),
	@ArtNumber varchar(100),
	@PartName nvarchar(100),
	@Specs nvarchar(1000)
AS
	INSERT INTO Parts(URL, BrandName, ArtNumber, PartName, Specs)
	VALUES (@URL, @BrandName, @ArtNumber, @PartName, @Specs)
GO



DROP PROCEDURE [dbo].[sp_GetAllParts]
GO

CREATE PROCEDURE [dbo].[sp_GetAllParts]
AS
	SELECT * FROM [TestCaseDb].[dbo].[Parts]
GO



DROP PROCEDURE [dbo].[sp_GetPartsByNumber]
GO

CREATE PROCEDURE [dbo].[sp_GetPartsByNumber]
	@ArtNumberToFind varchar(100)
AS
	SELECT * FROM [TestCaseDb].[dbo].[Parts]
	WHERE ArtNumber LIKE @ArtNumberToFind
GO



DROP PROCEDURE [dbo].[sp_GetPartsByName]
GO

CREATE PROCEDURE [dbo].[sp_GetPartsByName]
	@PartNameToFind varchar(100)
AS
	SELECT * FROM [TestCaseDb].[dbo].[Parts]
	WHERE PartName LIKE '%' + @PartNameToFind + '%'
GO