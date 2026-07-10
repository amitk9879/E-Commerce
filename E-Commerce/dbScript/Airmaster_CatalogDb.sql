USE [master]
GO
/****** Object:  Database [Airmaster_CatalogDb]    Script Date: 10-07-2026 15:19:44 ******/
CREATE DATABASE [Airmaster_CatalogDb]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'Airmaster_CatalogDb', FILENAME = N'C:\Users\amitk\Airmaster_CatalogDb.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'Airmaster_CatalogDb_log', FILENAME = N'C:\Users\amitk\Airmaster_CatalogDb_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [Airmaster_CatalogDb].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [Airmaster_CatalogDb] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [Airmaster_CatalogDb] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [Airmaster_CatalogDb] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [Airmaster_CatalogDb] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [Airmaster_CatalogDb] SET ARITHABORT OFF 
GO
ALTER DATABASE [Airmaster_CatalogDb] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [Airmaster_CatalogDb] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [Airmaster_CatalogDb] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [Airmaster_CatalogDb] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [Airmaster_CatalogDb] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [Airmaster_CatalogDb] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [Airmaster_CatalogDb] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [Airmaster_CatalogDb] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [Airmaster_CatalogDb] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [Airmaster_CatalogDb] SET  ENABLE_BROKER 
GO
ALTER DATABASE [Airmaster_CatalogDb] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [Airmaster_CatalogDb] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [Airmaster_CatalogDb] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [Airmaster_CatalogDb] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [Airmaster_CatalogDb] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [Airmaster_CatalogDb] SET READ_COMMITTED_SNAPSHOT ON 
GO
ALTER DATABASE [Airmaster_CatalogDb] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [Airmaster_CatalogDb] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [Airmaster_CatalogDb] SET  MULTI_USER 
GO
ALTER DATABASE [Airmaster_CatalogDb] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [Airmaster_CatalogDb] SET DB_CHAINING OFF 
GO
ALTER DATABASE [Airmaster_CatalogDb] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [Airmaster_CatalogDb] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
USE [Airmaster_CatalogDb]
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 10-07-2026 15:19:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Categories]    Script Date: 10-07-2026 15:19:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Categories](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Products]    Script Date: 10-07-2026 15:19:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Products](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](150) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[StockQuantity] [int] NOT NULL,
	[ImageUrl] [nvarchar](max) NOT NULL,
	[CategoryId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260707202332_InitialCreate', N'10.0.9')
GO
INSERT [dbo].[Categories] ([Id], [Name], [Description]) VALUES (N'f97b7563-b0f1-4aab-a3af-9309430d104b', N'Smart Thermostats & Controls', N'Ecosystem control interfaces and automated zone management devices.')
GO
INSERT [dbo].[Categories] ([Id], [Name], [Description]) VALUES (N'ec2abb50-dc4f-4734-9101-93437c7b9dea', N'Commercial VRV Systems', N'Variable Refrigerant Volume solutions engineered for scalable commercial workloads.')
GO
INSERT [dbo].[Categories] ([Id], [Name], [Description]) VALUES (N'd040fb16-48c3-44ed-b948-bb7a0fa3ec39', N'Residential Split ACs', N'High-efficiency ductless mini-splits and multi-zone heat pumps.')
GO
INSERT [dbo].[Categories] ([Id], [Name], [Description]) VALUES (N'b353efde-1516-4483-b359-cf89ddf35f4d', N'Air Purification & IAQ', N'Active whole-house filtration hardware protecting indoor air spaces.')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'b51552b6-13fb-4b29-b36c-0110daaea845', N'Daikin Aurora Low-Ambient Module Mark 21', N'Enhanced wall unit engineered to output 100% rated heating capacity down to -15°C climates. Engineered rigorously for testing compliance validation scenario block #19.', CAST(1249.25 AS Decimal(18, 2)), 72, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'b353efde-1516-4483-b359-cf89ddf35f4d')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'5ee9d92d-2b78-4129-b1bd-097f7e4d0f89', N'Daikin VRV IV-X Water-Cooled Core Mark 12', N'State-of-the-art commercial condenser leveraging liquid loops for extreme cooling efficiencies. Engineered rigorously for testing compliance validation scenario block #10.', CAST(8757.50 AS Decimal(18, 2)), 45, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'ec2abb50-dc4f-4734-9101-93437c7b9dea')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'e0290346-227d-411d-8ddb-09fb9868f3b7', N'Daikin One+ Smart Ecosystem Link Mark 45', N'Touchscreen smart control module with built-in geofencing automation scheduling engines. Engineered rigorously for testing compliance validation scenario block #43.', CAST(976.25 AS Decimal(18, 2)), 144, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'b353efde-1516-4483-b359-cf89ddf35f4d')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'391ad666-6cac-4ef7-89e4-11fc1d7861b2', N'Daikin Streamer Air Purifier Tower Mark 48', N'Standalone tower purification device blasting pollen and airborne mold using flash streamer discharge. Engineered rigorously for testing compliance validation scenario block #46.', CAST(1074.00 AS Decimal(18, 2)), 153, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'ec2abb50-dc4f-4734-9101-93437c7b9dea')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'e08adc27-25fb-4777-947e-1678253b0c20', N'Daikin Fit Heat Pump Slim Series Mark 23', N'Side-discharge smart inverter outdoor unit designed to optimize tight footprint footprints. Engineered rigorously for testing compliance validation scenario block #21.', CAST(2780.75 AS Decimal(18, 2)), 78, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'f97b7563-b0f1-4aab-a3af-9309430d104b')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'65e55375-380f-4d62-b261-170ce05514dd', N'Daikin One+ Smart Ecosystem Link Mark 15', N'Touchscreen smart control module with built-in geofencing automation scheduling engines. Engineered rigorously for testing compliance validation scenario block #13.', CAST(503.75 AS Decimal(18, 2)), 54, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'f97b7563-b0f1-4aab-a3af-9309430d104b')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'da3fbb47-1254-4413-944c-185501add59c', N'Daikin Aurora Low-Ambient Module Mark 31', N'Enhanced wall unit engineered to output 100% rated heating capacity down to -15°C climates. Engineered rigorously for testing compliance validation scenario block #29.', CAST(1406.75 AS Decimal(18, 2)), 102, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'f97b7563-b0f1-4aab-a3af-9309430d104b')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'e10c5693-e1e5-4c54-b0f7-2175fb0cc314', N'Daikin Fit Heat Pump Slim Series Mark 3', N'Side-discharge smart inverter outdoor unit designed to optimize tight footprint footprints. Engineered rigorously for testing compliance validation scenario block #1.', CAST(2465.75 AS Decimal(18, 2)), 18, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'f97b7563-b0f1-4aab-a3af-9309430d104b')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'91a796dc-194b-47f7-b57b-24dccb1079e0', N'Daikin Fit Heat Pump Slim Series Mark 13', N'Side-discharge smart inverter outdoor unit designed to optimize tight footprint footprints. Engineered rigorously for testing compliance validation scenario block #11.', CAST(2623.25 AS Decimal(18, 2)), 48, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'b353efde-1516-4483-b359-cf89ddf35f4d')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'5457741e-dad7-4ff4-8c4f-266c6251d771', N'Daikin Streamer Air Purifier Tower Mark 18', N'Standalone tower purification device blasting pollen and airborne mold using flash streamer discharge. Engineered rigorously for testing compliance validation scenario block #16.', CAST(601.50 AS Decimal(18, 2)), 63, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'd040fb16-48c3-44ed-b948-bb7a0fa3ec39')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'e7933358-8ec6-4e3c-a1c0-277b6e0b2bd3', N'Daikin One Touch Automation Hub Mark 20', N'Cost-effective secondary thermostat sub-panel extending cloud scheduling routines. Engineered rigorously for testing compliance validation scenario block #18.', CAST(472.50 AS Decimal(18, 2)), 69, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'ec2abb50-dc4f-4734-9101-93437c7b9dea')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'40cb5ec7-e88f-4e0e-bc47-291f43ae6428', N'Daikin Emura Premium High-Wall Mark 4', N'Award-winning architectural wall-mounted unit utilizing R-32 eco-friendly refrigerant matrices. Engineered rigorously for testing compliance validation scenario block #2.', CAST(930.50 AS Decimal(18, 2)), 21, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'ec2abb50-dc4f-4734-9101-93437c7b9dea')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'9481828c-2648-4256-b774-2fd7015c185d', N'Daikin VRV LIFE Multi-Zone Frame Mark 47', N'Flexible VRV platform variant optimizing split configuration loads for luxury residences. Engineered rigorously for testing compliance validation scenario block #45.', CAST(5908.75 AS Decimal(18, 2)), 150, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'f97b7563-b0f1-4aab-a3af-9309430d104b')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'2d2e09f0-ee0f-41ca-8467-312121ea4f3a', N'Daikin Cirra Dual-Zone Concept Mark 19', N'Multi-zone entry system combining sub-freezing structural heating resilience parameters. Engineered rigorously for testing compliance validation scenario block #17.', CAST(1417.75 AS Decimal(18, 2)), 66, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'f97b7563-b0f1-4aab-a3af-9309430d104b')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'fa8273a3-40c6-4d1c-8759-387957207bf8', N'Daikin Aurora Low-Ambient Module Mark 11', N'Enhanced wall unit engineered to output 100% rated heating capacity down to -15°C climates. Engineered rigorously for testing compliance validation scenario block #9.', CAST(1091.75 AS Decimal(18, 2)), 42, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'f97b7563-b0f1-4aab-a3af-9309430d104b')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'd492a833-4dbf-4210-85ef-3c40522def97', N'Daikin Emura Premium High-Wall Mark 14', N'Award-winning architectural wall-mounted unit utilizing R-32 eco-friendly refrigerant matrices. Engineered rigorously for testing compliance validation scenario block #12.', CAST(1088.00 AS Decimal(18, 2)), 51, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'd040fb16-48c3-44ed-b948-bb7a0fa3ec39')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'6635b016-49c6-405a-8d1f-3c4466d100c0', N'Daikin Aurora Low-Ambient Module Mark 51', N'Enhanced wall unit engineered to output 100% rated heating capacity down to -15°C climates. Engineered rigorously for testing compliance validation scenario block #49.', CAST(1721.75 AS Decimal(18, 2)), 162, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'f97b7563-b0f1-4aab-a3af-9309430d104b')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'377f05b0-9688-4806-9c42-3cd82c9ce7be', N'Daikin Fit Heat Pump Slim Series Mark 33', N'Side-discharge smart inverter outdoor unit designed to optimize tight footprint footprints. Engineered rigorously for testing compliance validation scenario block #31.', CAST(2938.25 AS Decimal(18, 2)), 108, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'b353efde-1516-4483-b359-cf89ddf35f4d')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'a6774fea-02d4-484f-b608-4ad4b1d30e5d', N'Daikin Cirra Dual-Zone Concept Mark 9', N'Multi-zone entry system combining sub-freezing structural heating resilience parameters. Engineered rigorously for testing compliance validation scenario block #7.', CAST(1260.25 AS Decimal(18, 2)), 36, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'b353efde-1516-4483-b359-cf89ddf35f4d')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'79292556-4565-45f3-84b8-5015d5efd1b6', N'Daikin One Touch Automation Hub Mark 50', N'Cost-effective secondary thermostat sub-panel extending cloud scheduling routines. Engineered rigorously for testing compliance validation scenario block #48.', CAST(945.00 AS Decimal(18, 2)), 159, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'd040fb16-48c3-44ed-b948-bb7a0fa3ec39')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'163e7a6e-898d-44e8-984a-52d101c36b59', N'Daikin VRV IV-X Water-Cooled Core Mark 32', N'State-of-the-art commercial condenser leveraging liquid loops for extreme cooling efficiencies. Engineered rigorously for testing compliance validation scenario block #30.', CAST(9072.50 AS Decimal(18, 2)), 105, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'ec2abb50-dc4f-4734-9101-93437c7b9dea')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'd3b83abc-eae5-41b9-830a-56c9b282495d', N'Daikin Emura Premium High-Wall Mark 44', N'Award-winning architectural wall-mounted unit utilizing R-32 eco-friendly refrigerant matrices. Engineered rigorously for testing compliance validation scenario block #42.', CAST(1560.50 AS Decimal(18, 2)), 141, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'ec2abb50-dc4f-4734-9101-93437c7b9dea')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'6de8481f-990a-49ae-a9c8-59c7b8375738', N'Daikin One Touch Automation Hub Mark 30', N'Cost-effective secondary thermostat sub-panel extending cloud scheduling routines. Engineered rigorously for testing compliance validation scenario block #28.', CAST(630.00 AS Decimal(18, 2)), 99, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'd040fb16-48c3-44ed-b948-bb7a0fa3ec39')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'f0739807-1155-493c-9d32-5b9d16b81853', N'Daikin VRV LIFE Multi-Zone Frame Mark 7', N'Flexible VRV platform variant optimizing split configuration loads for luxury residences. Engineered rigorously for testing compliance validation scenario block #5.', CAST(5278.75 AS Decimal(18, 2)), 30, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'f97b7563-b0f1-4aab-a3af-9309430d104b')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'c0119973-7135-4327-bf23-5e1c726e993d', N'Daikin Cirra Dual-Zone Concept Mark 39', N'Multi-zone entry system combining sub-freezing structural heating resilience parameters. Engineered rigorously for testing compliance validation scenario block #37.', CAST(1732.75 AS Decimal(18, 2)), 126, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'f97b7563-b0f1-4aab-a3af-9309430d104b')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'36a57262-c110-41f9-8417-600ed1305e06', N'Daikin Altherma Hydronic Split v Mark 6', N'Air-to-water heat pump supplying highly effective residential radiant floor heating loops. Engineered rigorously for testing compliance validation scenario block #4.', CAST(4183.00 AS Decimal(18, 2)), 27, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'd040fb16-48c3-44ed-b948-bb7a0fa3ec39')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'94a4f4e1-2a40-4062-b133-649a073802ec', N'Daikin Cirra Dual-Zone Concept Mark 29', N'Multi-zone entry system combining sub-freezing structural heating resilience parameters. Engineered rigorously for testing compliance validation scenario block #27.', CAST(1575.25 AS Decimal(18, 2)), 96, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'b353efde-1516-4483-b359-cf89ddf35f4d')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'b60123fe-5667-4830-aa39-6c5e71ebb0d7', N'Daikin One+ Smart Ecosystem Link Mark 25', N'Touchscreen smart control module with built-in geofencing automation scheduling engines. Engineered rigorously for testing compliance validation scenario block #23.', CAST(661.25 AS Decimal(18, 2)), 84, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'b353efde-1516-4483-b359-cf89ddf35f4d')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'45e9e789-efc3-45bd-a565-7162323c4e8d', N'Daikin VRV LIFE Multi-Zone Frame Mark 17', N'Flexible VRV platform variant optimizing split configuration loads for luxury residences. Engineered rigorously for testing compliance validation scenario block #15.', CAST(5436.25 AS Decimal(18, 2)), 60, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'b353efde-1516-4483-b359-cf89ddf35f4d')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'7e468ab3-16c4-427c-931e-724c9d10b2a1', N'Daikin One+ Smart Ecosystem Link Mark 35', N'Touchscreen smart control module with built-in geofencing automation scheduling engines. Engineered rigorously for testing compliance validation scenario block #33.', CAST(818.75 AS Decimal(18, 2)), 114, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'f97b7563-b0f1-4aab-a3af-9309430d104b')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'969bb719-f4b7-48f6-b3e0-72b6d1789929', N'Daikin Streamer Air Purifier Tower Mark 28', N'Standalone tower purification device blasting pollen and airborne mold using flash streamer discharge. Engineered rigorously for testing compliance validation scenario block #26.', CAST(759.00 AS Decimal(18, 2)), 93, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'ec2abb50-dc4f-4734-9101-93437c7b9dea')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'ecc11194-af56-42a3-9e57-92b448f0b291', N'Daikin VRV LIFE Multi-Zone Frame Mark 37', N'Flexible VRV platform variant optimizing split configuration loads for luxury residences. Engineered rigorously for testing compliance validation scenario block #35.', CAST(5751.25 AS Decimal(18, 2)), 120, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'b353efde-1516-4483-b359-cf89ddf35f4d')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'bb195ab0-d3dc-4f68-9962-986c7a9cfef6', N'Daikin Altherma Hydronic Split v Mark 26', N'Air-to-water heat pump supplying highly effective residential radiant floor heating loops. Engineered rigorously for testing compliance validation scenario block #24.', CAST(4498.00 AS Decimal(18, 2)), 87, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'd040fb16-48c3-44ed-b948-bb7a0fa3ec39')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'4305c132-7e98-44d7-b5af-a031bd084fb8', N'Daikin Altherma Hydronic Split v Mark 36', N'Air-to-water heat pump supplying highly effective residential radiant floor heating loops. Engineered rigorously for testing compliance validation scenario block #34.', CAST(4655.50 AS Decimal(18, 2)), 117, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'ec2abb50-dc4f-4734-9101-93437c7b9dea')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'301c062d-7e8b-4154-ba2c-a4ae806f9c37', N'Daikin Altherma Hydronic Split v Mark 46', N'Air-to-water heat pump supplying highly effective residential radiant floor heating loops. Engineered rigorously for testing compliance validation scenario block #44.', CAST(4813.00 AS Decimal(18, 2)), 147, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'd040fb16-48c3-44ed-b948-bb7a0fa3ec39')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'30cf1818-635a-4f4b-a8d6-a5a615dfec2d', N'Daikin Cirra Dual-Zone Concept Mark 49', N'Multi-zone entry system combining sub-freezing structural heating resilience parameters. Engineered rigorously for testing compliance validation scenario block #47.', CAST(1890.25 AS Decimal(18, 2)), 156, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'b353efde-1516-4483-b359-cf89ddf35f4d')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'b74d07c7-463b-4736-866b-aaa78b237ac8', N'Daikin VRV LIFE Multi-Zone Frame Mark 27', N'Flexible VRV platform variant optimizing split configuration loads for luxury residences. Engineered rigorously for testing compliance validation scenario block #25.', CAST(5593.75 AS Decimal(18, 2)), 90, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'f97b7563-b0f1-4aab-a3af-9309430d104b')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'a738f390-3aca-43a2-b6a9-acc4db1d218d', N'Daikin Aurora Low-Ambient Module Mark 41', N'Enhanced wall unit engineered to output 100% rated heating capacity down to -15°C climates. Engineered rigorously for testing compliance validation scenario block #39.', CAST(1564.25 AS Decimal(18, 2)), 132, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'b353efde-1516-4483-b359-cf89ddf35f4d')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'3d4af806-3ae0-4298-9982-b768ecd368c3', N'Daikin VRV IV-X Water-Cooled Core Mark 22', N'State-of-the-art commercial condenser leveraging liquid loops for extreme cooling efficiencies. Engineered rigorously for testing compliance validation scenario block #20.', CAST(8915.00 AS Decimal(18, 2)), 75, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'd040fb16-48c3-44ed-b948-bb7a0fa3ec39')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'4c5761f9-8174-421d-9378-c2f744733bec', N'Daikin VRV IV-X Water-Cooled Core Mark 42', N'State-of-the-art commercial condenser leveraging liquid loops for extreme cooling efficiencies. Engineered rigorously for testing compliance validation scenario block #40.', CAST(9230.00 AS Decimal(18, 2)), 135, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'd040fb16-48c3-44ed-b948-bb7a0fa3ec39')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'0d83bc63-7a47-4050-9660-c66565756cbc', N'Daikin One Touch Automation Hub Mark 10', N'Cost-effective secondary thermostat sub-panel extending cloud scheduling routines. Engineered rigorously for testing compliance validation scenario block #8.', CAST(315.00 AS Decimal(18, 2)), 39, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'd040fb16-48c3-44ed-b948-bb7a0fa3ec39')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'1c32691b-85c9-49d1-bcd3-e2cc91013c3c', N'Daikin Emura Premium High-Wall Mark 34', N'Award-winning architectural wall-mounted unit utilizing R-32 eco-friendly refrigerant matrices. Engineered rigorously for testing compliance validation scenario block #32.', CAST(1403.00 AS Decimal(18, 2)), 111, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'd040fb16-48c3-44ed-b948-bb7a0fa3ec39')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'5b30e2bf-3e70-4720-8cc8-ee9f0969397f', N'Daikin One Touch Automation Hub Mark 40', N'Cost-effective secondary thermostat sub-panel extending cloud scheduling routines. Engineered rigorously for testing compliance validation scenario block #38.', CAST(787.50 AS Decimal(18, 2)), 129, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'ec2abb50-dc4f-4734-9101-93437c7b9dea')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'f0f7c575-c90b-4481-b727-f08627b1a8d5', N'Daikin Emura Premium High-Wall Mark 24', N'Award-winning architectural wall-mounted unit utilizing R-32 eco-friendly refrigerant matrices. Engineered rigorously for testing compliance validation scenario block #22.', CAST(1245.50 AS Decimal(18, 2)), 81, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'ec2abb50-dc4f-4734-9101-93437c7b9dea')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'7e0c61f9-92ee-43a2-ae4b-f0e29fe09776', N'Daikin Altherma Hydronic Split v Mark 16', N'Air-to-water heat pump supplying highly effective residential radiant floor heating loops. Engineered rigorously for testing compliance validation scenario block #14.', CAST(4340.50 AS Decimal(18, 2)), 57, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'ec2abb50-dc4f-4734-9101-93437c7b9dea')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'ffc5b417-0800-4dc4-bf78-f685e841d130', N'Daikin VRV IV-X Water-Cooled Core Mark 52', N'State-of-the-art commercial condenser leveraging liquid loops for extreme cooling efficiencies. Engineered rigorously for testing compliance validation scenario block #50.', CAST(9387.50 AS Decimal(18, 2)), 165, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'ec2abb50-dc4f-4734-9101-93437c7b9dea')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'601f868c-a73a-4d59-b59f-f945b0a0688e', N'Daikin Streamer Air Purifier Tower Mark 8', N'Standalone tower purification device blasting pollen and airborne mold using flash streamer discharge. Engineered rigorously for testing compliance validation scenario block #6.', CAST(444.00 AS Decimal(18, 2)), 33, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'ec2abb50-dc4f-4734-9101-93437c7b9dea')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'9b479352-f922-44ff-afa2-fcf781586bb6', N'Daikin One+ Smart Ecosystem Link Mark 5', N'Touchscreen smart control module with built-in geofencing automation scheduling engines. Engineered rigorously for testing compliance validation scenario block #3.', CAST(346.25 AS Decimal(18, 2)), 24, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'b353efde-1516-4483-b359-cf89ddf35f4d')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'4c5058d6-01f6-4716-893b-ff06f943925b', N'Daikin Fit Heat Pump Slim Series Mark 43', N'Side-discharge smart inverter outdoor unit designed to optimize tight footprint footprints. Engineered rigorously for testing compliance validation scenario block #41.', CAST(3095.75 AS Decimal(18, 2)), 138, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'f97b7563-b0f1-4aab-a3af-9309430d104b')
GO
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [StockQuantity], [ImageUrl], [CategoryId]) VALUES (N'79687328-6cdc-47d7-9b8d-ff8dfc8c8713', N'Daikin Streamer Air Purifier Tower Mark 38', N'Standalone tower purification device blasting pollen and airborne mold using flash streamer discharge. Engineered rigorously for testing compliance validation scenario block #36.', CAST(916.50 AS Decimal(18, 2)), 123, N'https://t4.ftcdn.net/jpg/07/48/96/59/240_F_748965994_LOT3WvTIng2geSwhIMn1gN2pQkEkGrFT.jpg', N'd040fb16-48c3-44ed-b948-bb7a0fa3ec39')
GO
/****** Object:  Index [IX_Products_CategoryId]    Script Date: 10-07-2026 15:19:44 ******/
CREATE NONCLUSTERED INDEX [IX_Products_CategoryId] ON [dbo].[Products]
(
	[CategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Products_Name]    Script Date: 10-07-2026 15:19:44 ******/
CREATE NONCLUSTERED INDEX [IX_Products_Name] ON [dbo].[Products]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Products]  WITH CHECK ADD  CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Categories] ([Id])
GO
ALTER TABLE [dbo].[Products] CHECK CONSTRAINT [FK_Products_Categories_CategoryId]
GO
USE [master]
GO
ALTER DATABASE [Airmaster_CatalogDb] SET  READ_WRITE 
GO
