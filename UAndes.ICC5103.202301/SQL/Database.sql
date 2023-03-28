Create Database InscripcionesBrDb
GO

USE [InscripcionesBrDb]
GO

CREATE TABLE [dbo].[Persona](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Rut] [nvarchar](10) NULL,
	[Nombre] [nvarchar](50) NOT NULL,
	[FechaNacimiento] [date] NOT NULL,
	[Email] [nchar](50) NOT NULL,
	[Dirección] [nchar](50) NULL,
 CONSTRAINT [PK_Persona] PRIMARY KEY CLUSTERED(
	[Id] ASC
))
GO

CREATE TABLE [dbo].[Enajenacion](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CNE] [nvarchar](50) NOT NULL,
	[Comuna] [nvarchar](50) NOT NULL,
	[Manzana] [nchar](50) NOT NULL,
	[RolPredial] [nchar](50) NOT NULL,
	[Enajenantes] [nvarchar](MAX)  NULL,
	[Adquirientes] [nvarchar](MAX)  NULL,
	[Foja] [nchar](50) NOT NULL,
	[FechaInscripcion] [date] NOT NULL,
	[NumeroInscripcion] [nchar](50) NULL,
 CONSTRAINT [PK_Enajenacion] PRIMARY KEY CLUSTERED(
	[Id] ASC
))
GO

CREATE TABLE [dbo].[Multipropietario](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Comuna] [nvarchar](50) NOT NULL,
	[Manzana] [nchar](50) NOT NULL,
	[RolPredial] [nchar](50) NOT NULL,
	[RutPropietario] [nvarchar](10) NOT NULL,
	[PorcentajeDerechoPropietario] [nchar](10) NULL,
	[Foja] [nchar](50) NOT NULL,
	[AnoInscripcion] [date] NOT NULL,
	[NumeroInscripcion] [nchar](50) NULL,
	[FechaInscripcion] [date] NOT NULL,
	[AnoVigenciaInicial] [date] NOT NULL,
	[AnoVigenciaFinal] [date] NULL
 CONSTRAINT [PK_Multipropietario] PRIMARY KEY CLUSTERED(
	[Id] ASC
))
GO

USE [InscripcionesBrDb]
GO
SET IDENTITY_INSERT [dbo].[Persona] ON 
GO
INSERT [dbo].[Persona] ([Id], [Rut], [Nombre], [FechaNacimiento], [Email], [Dirección]) VALUES (1, N'10915348-6', N'Mario Abellan', CAST(N'1982-01-15' AS Date), N'marioabellan@gmail.com                            ', N'Galvarino Gallardo 1534                           ')
GO
SET IDENTITY_INSERT [dbo].[Persona] OFF
GO

