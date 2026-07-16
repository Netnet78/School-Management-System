using Xunit;
using SchoolManagement.Core.Features.Candidates.Models;
using SchoolManagement.Core.Features.Students.Models;
using SchoolManagement.Core.Features.Students.Enums;
using SchoolManagement.Core.Shared.Enums;
using System;

namespace SchoolManagement.Tests.Features.Candidates
{
    // Tests for the Candidate model's Clone() method.
    public class CandidateTests
    {
        // [Fact]: marks this as a single, parameter-less test.
        [Fact]
        public void Clone_ShouldCreateCopyOfCandidateWithSameValues()
        {
            // Arrange: build a fully-populated Candidate to clone.
            var candidate = new Candidate
            {
                Id = 42,
                FirstName = "Sok",
                LatinFirstName = "Sok",
                LastName = "Chan",
                LatinLastName = "Chan",
                Gender = Gender.Male,
                DateOfBirth = new DateOnly(2005, 5, 20), // DateOnly: a date without a time component
                SkillId = 5,
                CreatedAt = DateTime.UtcNow,
                BirthVillage = "Village A",
                BirthCommune = "Commune B",
                BirthDistrict = "District C",
                BirthProvince = "Province D",
                FatherName = "Father X",
                MotherName = "Mother Y",
                FatherOccupation = "Teacher",
                MotherOccupation = "Doctor",
                SiblingsCount = 3,
                Religion = "Buddhism",
                PhoneNumber = "012345678",
                ExamCenter = "Center Z",
                ExamDate = new DateOnly(2025, 8, 15),
                ExamTable = 12,
                ExamRoom = 4,
                FromSchool = "High School H",
                StayType = StudentStayType.Outside,
                OtherInfo = "No info",
                // Photo is a nested object — Clone() should deep-copy it (create a new instance).
                Photo = new StudentPhoto
                {
                    Id = 10,
                    Key = "photo_key_123",
                    LocalPath = "local_path_123",
                    FileStatus = FileStatus.Uploaded,
                    LastAttempt = DateTime.UtcNow
                }
            };

            // Act: create the clone.
            var clone = candidate.Clone();

            // Assert: the clone exists and is a different object in memory.
            Assert.NotNull(clone);
            // Assert.NotSame: checks that clone and candidate are NOT the same object reference.
            Assert.NotSame(candidate, clone);
            // Assert.Equal: verifies each scalar field was copied correctly.
            Assert.Equal(candidate.Id, clone.Id);
            Assert.Equal(candidate.FirstName, clone.FirstName);
            Assert.Equal(candidate.LatinFirstName, clone.LatinFirstName);
            Assert.Equal(candidate.LastName, clone.LastName);
            Assert.Equal(candidate.LatinLastName, clone.LatinLastName);
            Assert.Equal(candidate.FullName, clone.FullName);
            Assert.Equal(candidate.LatinFullName, clone.LatinFullName);
            Assert.Equal(candidate.Gender, clone.Gender);
            Assert.Equal(candidate.DateOfBirth, clone.DateOfBirth);
            Assert.Equal(candidate.Age, clone.Age);
            Assert.Equal(candidate.SkillId, clone.SkillId);
            Assert.Equal(candidate.Skill, clone.Skill);
            Assert.Equal(candidate.CreatedAt, clone.CreatedAt);
            Assert.Equal(candidate.BirthVillage, clone.BirthVillage);
            Assert.Equal(candidate.BirthCommune, clone.BirthCommune);
            Assert.Equal(candidate.BirthDistrict, clone.BirthDistrict);
            Assert.Equal(candidate.BirthProvince, clone.BirthProvince);
            Assert.Equal(candidate.FatherName, clone.FatherName);
            Assert.Equal(candidate.MotherName, clone.MotherName);
            Assert.Equal(candidate.FatherOccupation, clone.FatherOccupation);
            Assert.Equal(candidate.MotherOccupation, clone.MotherOccupation);
            Assert.Equal(candidate.SiblingsCount, clone.SiblingsCount);
            Assert.Equal(candidate.Religion, clone.Religion);
            Assert.Equal(candidate.PhoneNumber, clone.PhoneNumber);
            Assert.Equal(candidate.ExamCenter, clone.ExamCenter);
            Assert.Equal(candidate.ExamDate, clone.ExamDate);
            Assert.Equal(candidate.ExamTable, clone.ExamTable);
            Assert.Equal(candidate.ExamRoom, clone.ExamRoom);
            Assert.Equal(candidate.FromSchool, clone.FromSchool);
            Assert.Equal(candidate.StayType, clone.StayType);
            Assert.Equal(candidate.OtherInfo, clone.OtherInfo);
            Assert.Equal(candidate.Student, clone.Student);

            // Deep clone photo assert
            // The cloned Photo must be a new object (not the same reference) with identical field values.
            Assert.NotNull(clone.Photo);
            Assert.NotSame(candidate.Photo, clone.Photo);
            Assert.Equal(candidate.Photo.Id, clone.Photo.Id);
            Assert.Equal(candidate.Photo.Key, clone.Photo.Key);
            Assert.Equal(candidate.Photo.LocalPath, clone.Photo.LocalPath);
            Assert.Equal(candidate.Photo.FileStatus, clone.Photo.FileStatus);
            Assert.Equal(candidate.Photo.LastAttempt, clone.Photo.LastAttempt);
            // Assert.Same: the Photo's back-reference to its owner should point to the clone, not the original.
            Assert.Same(clone, clone.Photo.Student);
        }

        [Fact]
        public void Clone_WithNullPhoto_ShouldReturnCloneWithNullPhoto()
        {
            // Arrange: a candidate with no photo set.
            var candidate = new Candidate
            {
                FirstName = "Sok",
                LastName = "Chan",
                Photo = null
            };

            // Act
            var clone = candidate.Clone();

            // Assert: clone exists and its Photo is still null.
            Assert.NotNull(clone);
            Assert.Null(clone.Photo);
        }
    }
}
