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
            migrationBuilder.DropForeignKey(
                name: "FK_TutorApplication_AspNetUsers_ReviewedByUserId",
                table: "TutorApplication");

            migrationBuilder.DropForeignKey(
                name: "FK_TutorApplication_AspNetUsers_UserId",
                table: "TutorApplication");

            migrationBuilder.DropForeignKey(
                name: "FK_TutorApplicationAvailability_TutorApplication_TutorApplicationId",
                table: "TutorApplicationAvailability");

            migrationBuilder.DropForeignKey(
                name: "FK_TutorApplicationSubject_Subjects_SubjectId",
                table: "TutorApplicationSubject");

            migrationBuilder.DropForeignKey(
                name: "FK_TutorApplicationSubject_TutorApplication_TutorApplicationId",
                table: "TutorApplicationSubject");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TutorApplicationSubject",
                table: "TutorApplicationSubject");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TutorApplicationAvailability",
                table: "TutorApplicationAvailability");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TutorApplication",
                table: "TutorApplication");

            migrationBuilder.RenameTable(
                name: "TutorApplicationSubject",
                newName: "TutorApplicationSubjects");

            migrationBuilder.RenameTable(
                name: "TutorApplicationAvailability",
                newName: "TutorApplicationAvailabilities");

            migrationBuilder.RenameTable(
                name: "TutorApplication",
                newName: "TutorApplications");

            migrationBuilder.RenameIndex(
                name: "IX_TutorApplicationSubject_SubjectId",
                table: "TutorApplicationSubjects",
                newName: "IX_TutorApplicationSubjects_SubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_TutorApplicationAvailability_TutorApplicationId_DayOfWeek_StartTime_EndTime",
                table: "TutorApplicationAvailabilities",
                newName: "IX_TutorApplicationAvailabilities_TutorApplicationId_DayOfWeek_StartTime_EndTime");

            migrationBuilder.RenameIndex(
                name: "IX_TutorApplication_UserId",
                table: "TutorApplications",
                newName: "IX_TutorApplications_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TutorApplication_ReviewedByUserId",
                table: "TutorApplications",
                newName: "IX_TutorApplications_ReviewedByUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TutorApplicationSubjects",
                table: "TutorApplicationSubjects",
                columns: new[] { "TutorApplicationId", "SubjectId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_TutorApplicationAvailabilities",
                table: "TutorApplicationAvailabilities",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TutorApplications",
                table: "TutorApplications",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TutorApplicationAvailabilities_TutorApplications_TutorApplicationId",
                table: "TutorApplicationAvailabilities",
                column: "TutorApplicationId",
                principalTable: "TutorApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TutorApplications_AspNetUsers_ReviewedByUserId",
                table: "TutorApplications",
                column: "ReviewedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TutorApplications_AspNetUsers_UserId",
                table: "TutorApplications",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TutorApplicationSubjects_Subjects_SubjectId",
                table: "TutorApplicationSubjects",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TutorApplicationSubjects_TutorApplications_TutorApplicationId",
                table: "TutorApplicationSubjects",
                column: "TutorApplicationId",
                principalTable: "TutorApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TutorApplicationAvailabilities_TutorApplications_TutorApplicationId",
                table: "TutorApplicationAvailabilities");

            migrationBuilder.DropForeignKey(
                name: "FK_TutorApplications_AspNetUsers_ReviewedByUserId",
                table: "TutorApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_TutorApplications_AspNetUsers_UserId",
                table: "TutorApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_TutorApplicationSubjects_Subjects_SubjectId",
                table: "TutorApplicationSubjects");

            migrationBuilder.DropForeignKey(
                name: "FK_TutorApplicationSubjects_TutorApplications_TutorApplicationId",
                table: "TutorApplicationSubjects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TutorApplicationSubjects",
                table: "TutorApplicationSubjects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TutorApplications",
                table: "TutorApplications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TutorApplicationAvailabilities",
                table: "TutorApplicationAvailabilities");

            migrationBuilder.RenameTable(
                name: "TutorApplicationSubjects",
                newName: "TutorApplicationSubject");

            migrationBuilder.RenameTable(
                name: "TutorApplications",
                newName: "TutorApplication");

            migrationBuilder.RenameTable(
                name: "TutorApplicationAvailabilities",
                newName: "TutorApplicationAvailability");

            migrationBuilder.RenameIndex(
                name: "IX_TutorApplicationSubjects_SubjectId",
                table: "TutorApplicationSubject",
                newName: "IX_TutorApplicationSubject_SubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_TutorApplications_UserId",
                table: "TutorApplication",
                newName: "IX_TutorApplication_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TutorApplications_ReviewedByUserId",
                table: "TutorApplication",
                newName: "IX_TutorApplication_ReviewedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_TutorApplicationAvailabilities_TutorApplicationId_DayOfWeek_StartTime_EndTime",
                table: "TutorApplicationAvailability",
                newName: "IX_TutorApplicationAvailability_TutorApplicationId_DayOfWeek_StartTime_EndTime");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TutorApplicationSubject",
                table: "TutorApplicationSubject",
                columns: new[] { "TutorApplicationId", "SubjectId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_TutorApplication",
                table: "TutorApplication",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TutorApplicationAvailability",
                table: "TutorApplicationAvailability",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TutorApplication_AspNetUsers_ReviewedByUserId",
                table: "TutorApplication",
                column: "ReviewedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TutorApplication_AspNetUsers_UserId",
                table: "TutorApplication",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TutorApplicationAvailability_TutorApplication_TutorApplicationId",
                table: "TutorApplicationAvailability",
                column: "TutorApplicationId",
                principalTable: "TutorApplication",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TutorApplicationSubject_Subjects_SubjectId",
                table: "TutorApplicationSubject",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TutorApplicationSubject_TutorApplication_TutorApplicationId",
                table: "TutorApplicationSubject",
                column: "TutorApplicationId",
                principalTable: "TutorApplication",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
