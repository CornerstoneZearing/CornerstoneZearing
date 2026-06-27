-- ----------------------
-- Events Calendar Schema
-- ----------------------

CREATE TABLE [Events] (
	[EventID] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Location] [nvarchar](200) NOT NULL,
	[StartDateTime] [datetime2](7) NOT NULL,
	[EndDateTime] [datetime2](7) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[IsAllDay] [bit] NOT NULL,
	[RecurrenceType] [int] NOT NULL,
	[RecurrenceInterval] [int] NOT NULL,
	[RecurSunday] [bit] NOT NULL,
	[RecurMonday] [bit] NOT NULL,
	[RecurTuesday] [bit] NOT NULL,
	[RecurWednesday] [bit] NOT NULL,
	[RecurThursday] [bit] NOT NULL,
	[RecurFriday] [bit] NOT NULL,
	[RecurSaturday] [bit] NOT NULL,
	[MonthlyYearlyPattern] [int] NOT NULL,
	[RecurrenceEndDate] [datetime2](7) NULL,
	[DateCreated] [datetime2](7) NOT NULL,
	[DateModified] [datetime2](7) NOT NULL,
	[IsPrivate] [bit] NOT NULL,
CONSTRAINT [PK_Events] PRIMARY KEY CLUSTERED ([EventID] ASC) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- Constraints

ALTER TABLE [Events] 
ADD DEFAULT (0) FOR [IsPrivate]
GO