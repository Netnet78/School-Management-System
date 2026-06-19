## Authorization Handler Pattern Documentation

### Overview
The authorization handler pattern provides extensible, role-based access control for different modules in the School Management System. Each handler is responsible for evaluating permissions for a specific resource type.

### Handler Registration & Execution Flow

```
User Request
    ↓
AuthorizationService.AuthorizeAsync()
    ↓
Iterate through all registered IAuthorizationHandler implementations
    ↓
Each handler evaluates: user role, permissions, and resource-specific rules
    ↓
First handler returning true = Request authorized
    ↓
No handlers returning true = Request denied
```

### Available Handlers & Responsibilities

#### 1. **StudentAuthorizationHandler**
- **Applies To**: Student resources
- **Rules**:
  - ✅ Admins: Full access
  - ✅ Head Teachers: Can access students in their department
  - ✅ Teachers: Can access students in their classes
  - ❌ Others: Denied

#### 2. **AttendanceAuthorizationHandler**
- **Applies To**: Attendance records
- **Rules**:
  - ✅ Admins: Full access
  - ✅ Head Teachers: Can access attendance in their department
  - ✅ Teachers: Can access attendance for their classes
  - ❌ Others: Denied

#### 3. **ClassAuthorizationHandler** (NEW)
- **Applies To**: Class resources
- **Rules**:
  - ✅ Admins: Full access
  - ✅ Head Teachers: Can manage classes in their department
  - ✅ Teachers: Can only view classes they teach
  - ❌ Others: Denied

#### 4. **EmployeeAuthorizationHandler** (NEW)
- **Applies To**: Employee resources
- **Rules**:
  - ✅ Admins: Full access
  - ✅ Head Teachers: Can view employees in their department
  - ❌ Teachers: Denied (except for global operations)
  - ❌ Others: Denied

#### 5. **CandidateAuthorizationHandler** (NEW)
- **Applies To**: Candidate resources
- **Rules**:
  - ✅ Admins: Full access
  - ❌ All others: Denied (candidates are admin-only)

#### 6. **GradeAuthorizationHandler** (NEW)
- **Applies To**: Grade resources
- **Rules**:
  - ✅ Admins: Full access
  - ✅ Head Teachers: Can manage grades in their department
  - ✅ Teachers: Can manage grades for their generation
  - ❌ Others: Denied

#### 7. **ExamAuthorizationHandler** (NEW)
- **Applies To**: Exam resources
- **Rules**:
  - ✅ Admins: Full access
  - ✅ Head Teachers: Can manage exams in their department
  - ✅ Teachers: Can manage exams for their generation
  - ❌ Others: Denied

#### 8. **ScoreAuthorizationHandler** (NEW)
- **Applies To**: Score resources
- **Rules**:
  - ✅ Admins: Full access
  - ✅ Head Teachers: Can manage scores in their department
  - ✅ Teachers: Can manage scores for students in their classes
  - ❌ Others: Denied

### How to Create a New Handler

#### Step 1: Create the Handler Class
```csharp

using SchoolManagement.Core.Interfaces.Application;


namespace SchoolManagement.Application.Policies
{
    /// <summary>
    /// Handles authorization for [Resource]-related operations.
    /// [Brief description of access rules]
    /// </summary>
    public class YourResourceAuthorizationHandler : IAuthorizationHandler
    {
        public async Task<bool> HandleAsync(
            User? user,
            object? resource,
            OperatorMode operatorMode,
            params PermissionType[] requirements)
        {
            // 1. Check if user exists
            if (user == null) return false;

            // 2. Admins always have access
            if (user.IsAdmin())
                return true;

            // 3. Check if user has required permissions
            if (!user.HasValidPermissions(operatorMode, requirements))
                return false;

            // 4. If no specific resource, just return based on permissions
            if (resource == null)
                return true;

            // 5. Type-check the resource
            if (resource is not YourResourceType yourResource)
                return false;

            // 6. Implement resource-specific rules
            if (user.IsHeadTeacher())
            {
                // Head teacher logic
                return user.Employee?.DepartmentId == yourResource.DepartmentId;
            }

            // 7. Implement teacher/other role logic
            return false;
        }
    }
}
```

#### Step 2: Register in DependencyInjection.cs
```csharp
// In Application/SchoolManagement.Application/DependencyInjection.cs

services.AddScoped<IAuthorizationHandler, YourResourceAuthorizationHandler>();
```

#### Step 3: Use in Services/ViewModels
```csharp
// Check authorization before operations
bool authorized = await _authorizationService.AuthorizeAsync(
    resource: student,
    operatorMode: OperatorMode.AND,
    permissions: PermissionType.EditStudents
);

if (!authorized)
{
    _messageService.Show(
        "You don't have permission to edit this student",
        "Access Denied",
        icon: MessageIcon.Hand
    );
    return;
}
```

### Authorization Service Usage

#### Global (No Resource-Specific) Checks
```csharp
// Check if user can manage any students
bool canManage = await _authorizationService.AuthorizeAsync(
    resource: null,
    permission: PermissionType.ManageAttendances
);
```

#### Resource-Specific Checks
```csharp
// Check if user can manage THIS specific student
bool canEditStudent = await _authorizationService.AuthorizeAsync(
    resource: student,
    permission: PermissionType.EditStudents
);
```

#### Multiple Permission Requirements (AND)
```csharp
// User must have ALL permissions
bool canDo = await _authorizationService.AuthorizeAsync(
    resource: null,
    operatorMode: OperatorMode.AND,
    permissions: PermissionType.ViewStudents, PermissionType.EditStudents
);
```

#### Multiple Permission Requirements (OR)
```csharp
// User needs ANY of these permissions
bool canAccess = await _authorizationService.AuthorizeAsync(
    resource: null,
    operatorMode: OperatorMode.OR,
    permissions: PermissionType.ViewStudents, PermissionType.EditStudents
);
```

### Permission Types Reference

```csharp
public enum PermissionType
{
    // Student Permissions
    ViewStudents,
    InsertStudents,
    EditStudents,
    DeleteStudents,

    // Class Permissions
    ViewClasses,
    InsertClasses,
    EditClasses,
    DeleteClasses,

    // Employee & Attendance
    ManageEmployees,
    ManageAttendances,

    // Candidate Management
    ManageCandidates,
}
```

### How Authorization Checks Work in Views

#### Example: DashboardViewModel
```csharp
// Check if user can view attendance data
bool authorized = await _authorizationService.AuthorizeAsync(
    null,
    PermissionType.ManageAttendances
);

if (!authorized)
{
    _messageService.Show(
        "You don't have permission to view attendance data",
        "Access Denied",
        icon: MessageIcon.Hand
    );
    return;
}

// Load data only if authorized
await LoadAttendanceChart();
```

### Testing Authorization Handlers

```csharp
[TestClass]
public class StudentAuthorizationHandlerTests
{
    [TestMethod]
    public async Task Admin_Should_Have_Full_Access()
    {
        // Arrange
        var adminUser = new User { Role = new Role { Name = "Admin" } };
        var handler = new StudentAuthorizationHandler();

        // Act
        var result = await handler.HandleAsync(
            adminUser,
            new Student(),
            OperatorMode.AND,
            PermissionType.ViewStudents
        );

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task NonAdmin_Without_Permission_Should_Be_Denied()
    {
        // Arrange
        var teacher = new User
        {
            Role = new Role
            {
                Name = "Teacher",
                Permissions = new List<Permission>() // Empty permissions
            }
        };
        var handler = new StudentAuthorizationHandler();

        // Act
        var result = await handler.HandleAsync(
            teacher,
            null,
            OperatorMode.AND,
            PermissionType.ViewStudents
        );

        // Assert
        Assert.IsFalse(result);
    }
}
```

### Checklist for Complete Authorization Coverage

- [x] StudentAuthorizationHandler - Student module
- [x] AttendanceAuthorizationHandler - Attendance module
- [x] ClassAuthorizationHandler - Class management
- [x] EmployeeAuthorizationHandler - Employee management
- [x] CandidateAuthorizationHandler - Candidate processing
- [x] GradeAuthorizationHandler - Grade management
- [x] ExamAuthorizationHandler - Exam management
- [x] ScoreAuthorizationHandler - Score management

### Modules Needing Additional Handlers (If Added)

If you add new modules, ensure handlers are created for:
- Subject Management
- Department Management
- Skill Management
- Notification Management
- Role/Permission Management (Admin-only)

### Best Practices

1. **Always check user existence first** - Null check prevents null reference exceptions
2. **Admin bypass is first** - Performance optimization (don't check permissions for admins)
3. **Permission check before resource check** - Fail fast on missing permissions
4. **Resource null handling** - Global operations (no resource) should still check permissions
5. **Type safety** - Use pattern matching (`is not YourType`) for type safety
6. **Department-based access** - Leverage `employee.DepartmentId` for head teachers
7. **Class-based access** - Use `employee.Classes` for teacher-specific access
8. **Consistent naming** - Handler name = `{Resource}AuthorizationHandler`

