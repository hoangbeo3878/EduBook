using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduBook.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTutorApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TutorApplication",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Qualifications = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Introduction = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AdminNote = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TutorApplication", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TutorApplication_AspNetUsers_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TutorApplication_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TutorApplicationAvailability",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TutorApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TutorApplicationAvailability", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TutorApplicationAvailability_TutorApplication_TutorApplicationId",
                        column: x => x.TutorApplicationId,
                        principalTable: "TutorApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TutorApplicationSubject",
                columns: table => new
                {
                    TutorApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TutorApplicationSubject", x => new { x.TutorApplicationId, x.SubjectId });
                    table.ForeignKey(
                        name: "FK_TutorApplicationSubject_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TutorApplicationSubject_TutorApplication_TutorApplicationId",
                        column: x => x.TutorApplicationId,
                        principalTable: "TutorApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TutorApplication_ReviewedByUserId",
                table: "TutorApplication",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TutorApplication_UserId",
                table: "TutorApplication",
                column: "UserId",
                unique: true,
                filter: "[Status] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TutorApplicationAvailability_TutorApplicationId_DayOfWeek_StartTime_EndTime",
                table: "TutorApplicationAvailability",
                columns: new[] { "TutorApplicationId", "DayOfWeek", "StartTime", "EndTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TutorApplicationSubject_SubjectId",
                table: "TutorApplicationSubject",
                column: "SubjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TutorApplicationAvailability");

            migrationBuilder.DropTable(
                name: "TutorApplicationSubject");

            migrationBuilder.DropTable(
                name: "TutorApplication");
        }
    }
}
