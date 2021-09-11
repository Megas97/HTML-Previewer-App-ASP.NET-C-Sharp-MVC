CREATE TABLE [dbo].[HTMLCode] (
    [id]      INT            IDENTITY (1, 1) NOT NULL,
    [html]    NVARCHAR (MAX) NOT NULL,
    [created] DATETIME       NOT NULL,
    [edited]  DATETIME       NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
);