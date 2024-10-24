CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

CREATE TABLE "Counts" (
    "Id" serial NOT NULL,
    "Code" text NOT NULL,
    "Description" text NOT NULL,
    "Value" integer NOT NULL,
    CONSTRAINT "PK_Counts" PRIMARY KEY ("Id")
);

CREATE TABLE "Grades" (
    "Id" serial NOT NULL,
    "Name" text NOT NULL,
    "Code" text NOT NULL,
    "Description" text NOT NULL,
    CONSTRAINT "PK_Grades" PRIMARY KEY ("Id")
);

CREATE TABLE "Persents" (
    "Id" serial NOT NULL,
    "Code" text NOT NULL,
    "Description" text NOT NULL,
    "Value" double precision NOT NULL,
    CONSTRAINT "PK_Persents" PRIMARY KEY ("Id")
);

CREATE TABLE "Roles" (
    "Id" serial NOT NULL,
    "Name" text NOT NULL,
    "Code" text NOT NULL,
    "Description" text NOT NULL,
    CONSTRAINT "PK_Roles" PRIMARY KEY ("Id")
);

CREATE TABLE "AnswerTimes" (
    "Id" serial NOT NULL,
    "GradeId" integer NOT NULL,
    "Average" double precision NOT NULL,
    "Min" double precision NOT NULL,
    "Max" double precision NOT NULL,
    CONSTRAINT "PK_AnswerTimes" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AnswerTimes_Grades_GradeId" FOREIGN KEY ("GradeId") REFERENCES "Grades" ("Id") ON DELETE CASCADE
);

CREATE TABLE "GradeLevels" (
    "Id" serial NOT NULL,
    "GradeId" integer NOT NULL,
    "Level" double precision NOT NULL,
    CONSTRAINT "PK_GradeLevels" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_GradeLevels_Grades_GradeId" FOREIGN KEY ("GradeId") REFERENCES "Grades" ("Id") ON DELETE CASCADE
);

CREATE TABLE "GradeRelations" (
    "Id" serial NOT NULL,
    "StartId" integer,
    "EndId" integer,
    CONSTRAINT "PK_GradeRelations" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_GradeRelations_Grades_EndId" FOREIGN KEY ("EndId") REFERENCES "Grades" ("Id"),
    CONSTRAINT "FK_GradeRelations_Grades_StartId" FOREIGN KEY ("StartId") REFERENCES "Grades" ("Id")
);

CREATE TABLE "Weights" (
    "Id" serial NOT NULL,
    "GradeId" integer NOT NULL,
    "Min" double precision NOT NULL,
    "Max" double precision NOT NULL,
    CONSTRAINT "PK_Weights" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Weights_Grades_GradeId" FOREIGN KEY ("GradeId") REFERENCES "Grades" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Topics" (
    "Id" serial NOT NULL,
    "Name" text NOT NULL,
    "Description" text NOT NULL,
    "RoleId" integer NOT NULL,
    CONSTRAINT "PK_Topics" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Topics_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Users" (
    "Id" serial NOT NULL,
    "UserName" text NOT NULL,
    "PasswordHash" text NOT NULL,
    "RegistrationDate" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
);

CREATE TABLE "Questions" (
    "Id" serial NOT NULL,
    "Content" text NOT NULL,
    "TopicId" integer NOT NULL,
    "Weight" double precision NOT NULL,
    "IsMultipleChoice" boolean NOT NULL,
    CONSTRAINT "PK_Questions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Questions_Topics_TopicId" FOREIGN KEY ("TopicId") REFERENCES "Topics" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Sessions" (
    "Id" serial NOT NULL,
    "UserId" integer NOT NULL,
    "StartTime" timestamp with time zone NOT NULL,
    "EndTime" timestamp with time zone,
    "Score" double precision NOT NULL,
    "RoleId" integer NOT NULL,
    CONSTRAINT "PK_Sessions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Sessions_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Sessions_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Answers" (
    "Id" serial NOT NULL,
    "QuestionId" integer NOT NULL,
    "Content" text NOT NULL,
    "IsCorrect" boolean NOT NULL,
    CONSTRAINT "PK_Answers" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Answers_Questions_QuestionId" FOREIGN KEY ("QuestionId") REFERENCES "Questions" ("Id") ON DELETE CASCADE
);

CREATE TABLE "UserAnswers" (
    "Id" serial NOT NULL,
    "SessionId" integer NOT NULL,
    "QuestionId" integer NOT NULL,
    "TimeSpent" double precision NOT NULL,
    "Score" double precision NOT NULL,
    "AnswerTime" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_UserAnswers" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_UserAnswers_Questions_QuestionId" FOREIGN KEY ("QuestionId") REFERENCES "Questions" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_UserAnswers_Sessions_SessionId" FOREIGN KEY ("SessionId") REFERENCES "Sessions" ("Id") ON DELETE CASCADE
);

CREATE TABLE "UserTopics" (
    "Id" serial NOT NULL,
    "SessionId" integer NOT NULL,
    "TopicId" integer NOT NULL,
    "GradeId" integer NOT NULL,
    "Weight" double precision NOT NULL,
    "IsFinished" boolean NOT NULL,
    "WasPrevious" boolean NOT NULL,
    "Actual" boolean NOT NULL,
    "Count" integer NOT NULL,
    CONSTRAINT "PK_UserTopics" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_UserTopics_Grades_GradeId" FOREIGN KEY ("GradeId") REFERENCES "Grades" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_UserTopics_Sessions_SessionId" FOREIGN KEY ("SessionId") REFERENCES "Sessions" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_UserTopics_Topics_TopicId" FOREIGN KEY ("TopicId") REFERENCES "Topics" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_Answers_QuestionId" ON "Answers" ("QuestionId");

CREATE INDEX "IX_AnswerTimes_GradeId" ON "AnswerTimes" ("GradeId");

CREATE INDEX "IX_GradeLevels_GradeId" ON "GradeLevels" ("GradeId");

CREATE INDEX "IX_GradeRelations_EndId" ON "GradeRelations" ("EndId");

CREATE INDEX "IX_GradeRelations_StartId" ON "GradeRelations" ("StartId");

CREATE INDEX "IX_Questions_TopicId" ON "Questions" ("TopicId");

CREATE INDEX "IX_Sessions_UserId" ON "Sessions" ("UserId");

CREATE INDEX "IX_Topics_RoleId" ON "Topics" ("RoleId");

CREATE INDEX "IX_UserAnswers_QuestionId" ON "UserAnswers" ("QuestionId");

CREATE INDEX "IX_UserAnswers_SessionId" ON "UserAnswers" ("SessionId");

CREATE INDEX "IX_Users_RoleId" ON "Users" ("RoleId");

CREATE INDEX "IX_UserTopics_GradeId" ON "UserTopics" ("GradeId");

CREATE INDEX "IX_UserTopics_SessionId" ON "UserTopics" ("SessionId");

CREATE INDEX "IX_UserTopics_TopicId" ON "UserTopics" ("TopicId");

CREATE INDEX "IX_Weights_GradeId" ON "Weights" ("GradeId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20241001143451_InitialCreate', '8.0.8');

/*  
Конфигурация Counts 
*/  

INSERT INTO "Counts" ("Id", "Code", "Description", "Value") VALUES (1, 'Raise', 'Количество вопросов для повышения уровня', 3);
INSERT INTO "Counts" ("Id", "Code", "Description", "Value") VALUES (2, 'Question', 'Количество вопросов в топике', 10);

/*  
Конфигурация Grades 
*/  

INSERT INTO "Grades" ("Id", "Name", "Code", "Description") VALUES (1, 'Junior', 'Junior', 'Grade of Junior');
INSERT INTO "Grades" ("Id", "Name", "Code", "Description") VALUES (2, 'Middle', 'Middle', 'Grade of Middle');
INSERT INTO "Grades" ("Id", "Name", "Code", "Description") VALUES (3, 'Senior', 'Senior', 'Grade of Senior');

/*  
Конфигурация Persents 
*/  

INSERT INTO "Persents" ("Id", "Code", "Description", "Value") VALUES (1, 'GainWeight', 'Верхняя граница веса', 0.2);
INSERT INTO "Persents" ("Id", "Code", "Description", "Value") VALUES (2, 'LessWeight', 'Нижняя граница веса', 0.1);
INSERT INTO "Persents" ("Id", "Code", "Description", "Value") VALUES (3, 'RaiseData', 'Процент повышения уровня', 0.2);
INSERT INTO "Persents" ("Id", "Code", "Description", "Value") VALUES (4, 'RaiseLevel', 'Процент веса при повышении уровня', 0.3);

/*  
Конфигурация Roles 
*/  

INSERT INTO "Roles" ("Id", "Name", "Code", "Description") VALUES (1, 'QA', 'QA', 'Quality assurance');

/*  
Конфигурация GradeLevels 
*/  

INSERT INTO "GradeLevels" ("Id", "GradeId", "Level") VALUES (1, 1, 0.1);
INSERT INTO "GradeLevels" ("Id", "GradeId", "Level") VALUES (2, 2, 0.4);
INSERT INTO "GradeLevels" ("Id", "GradeId", "Level") VALUES (3, 3, 0.5);

/*  
Конфигурация GradeRelations 
*/  

INSERT INTO "GradeRelations" ("Id", "StartId", "EndId") VALUES (2, null, 1);
INSERT INTO "GradeRelations" ("Id", "StartId", "EndId") VALUES (3, 1, 2);
INSERT INTO "GradeRelations" ("Id", "StartId", "EndId") VALUES (4, 2, 3);
INSERT INTO "GradeRelations" ("Id", "StartId", "EndId") VALUES (5, 3, null);

/*  
Конфигурация AnswerTimes 
*/  

INSERT INTO "AnswerTimes" ("Id", "GradeId", "Average", "Min", "Max") VALUES (1, 1, 60, 0.95, 1.05);
INSERT INTO "AnswerTimes" ("Id", "GradeId", "Average", "Min", "Max") VALUES (2, 2, 90, 0.93, 1.07);
INSERT INTO "AnswerTimes" ("Id", "GradeId", "Average", "Min", "Max") VALUES (3, 3, 120, 0.9, 1.1);


/*  
Конфигурация Weights 
*/ 

INSERT INTO "Weights" ("Id", "GradeId", "Min", "Max") VALUES (1, 1, 0.2, 0.4);
INSERT INTO "Weights" ("Id", "GradeId", "Min", "Max") VALUES (2, 2, 0.4, 0.6);
INSERT INTO "Weights" ("Id", "GradeId", "Min", "Max") VALUES (3, 3, 0.6, 0.8);


COMMIT;

