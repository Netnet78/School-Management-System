# Authorization Handlers Implementation Guide

## Quick Start

You now have 8 authorization handlers covering all major modules:

1. **StudentAuthorizationHandler** ✅ - Student module
2. **AttendanceAuthorizationHandler** ✅ - Attendance module  
3. **ClassAuthorizationHandler** ✅ - Class management
4. **EmployeeAuthorizationHandler** ✅ - Employee management
5. **CandidateAuthorizationHandler** ✅ - Candidate processing
6. **GradeAuthorizationHandler** ✅ - Grade management
7. **ExamAuthorizationHandler** ✅ - Exam management
8. **ScoreAuthorizationHandler** ✅ - Score management

All handlers are registered in `DependencyInjection.cs` as scoped services.

---

## Common Usage Patterns

### Pattern 1: Check Before Loading Data
```csharp
// In your ViewModel or Service
private async Task LoadClassesAsync()
{
    // Check if user can view classes
    bool authorized = await _authorizationService.AuthorizeAsync(
        resource: null,
        permission: PermissionType.ViewClasses
    );

    if (!authorized)
    {
        _messageService.Show(
            "You don't have permission to view classes",
            "Access Denied",
            icon: MessageIcon.Hand
        );
        return;
    }

    // Load and display classes
    var response = await _classService.GetAllAsync();
    // ... display data
}
```

### Pattern 2: Check Before Modifying Resource
```csharp
private async Task DeleteStudentAsync(Student student)
{
    // Check if user can delete THIS student
    bool authorized = await _authorizationService.AuthorizeAsync(
        resource: student,
        permission: PermissionType.DeleteStudents
    );

    if (!authorized)
    {
        _messageService.Show(
            "You don't have permission to delete this student",
            "Access Denied",
            icon: MessageIcon.Hand
        );
        return;
    }

    // Proceed with deletion
    await _studentService.DeleteAsync(student);
}
```

### Pattern 3: Disable UI Elements Based on Permissions
```csharp
public partial class ClassViewModel : ObservableObject, IViewModel
{
    private readonly IAuthorizationService _authorizationService;

    [ObservableProperty]
    private bool canEditClasses = false;

    [ObservableProperty]
    private bool canDeleteClasses = false;

    private async Task LoadPermissionsAsync()
    {
        CanEditClasses = await _authorizationService.AuthorizeAsync(
            null,
            PermissionType.EditClasses
        );

        CanDeleteClasses = await _authorizationService.AuthorizeAsync(
            null,
            PermissionType.DeleteClasses
        );
    }

    public async Task LoadAsync()
    {
        await LoadPermissionsAsync();
        // ... load data
    }
}
```

**XAML:**
```xaml
<Button Command="{Binding EditCommand}" IsEnabled="{Binding CanEditClasses}">
    Edit Class
</Button>
<Button Command="{Binding DeleteCommand}" IsEnabled="{Binding CanDeleteClasses}">
    Delete Class
</Button>
```

### Pattern 4: Authorization in Service Layer
```csharp
public class ClassService : CrudServiceBase<Class>
{
    private readonly IAuthorizationService _authorizationService;

    public async Task<ReturnResponse> DeleteAsync(Class @class)
    {
        // Check authorization before allowing deletion
        bool authorized = await _authorizationService.AuthorizeAsync(
            resource: @class,
            permission: PermissionType.DeleteClasses
        );

        if (!authorized)
        {
            return new()
            {
                Status = Status.Failed,
                Message = "You don't have permission to delete this class"
            };
        }

        return await base.DeleteAsync(@class);
    }
}
```

---

## How Each Handler Works

### StudentAuthorizationHandler
**Resource**: `Student` object

**Access Rules**:
- **Admin**: ✅ Full access
- **Head Teacher**: ✅ Can access students in their department
- **Teacher**: ✅ Can access students they teach
- **Others**: ❌ Denied

**When to use**:
```csharp
await _authorizationService.AuthorizeAsync(
    resource: student,  // Specific student or null for global
    permission: PermissionType.EditStudents
);
```

---

### AttendanceAuthorizationHandler
**Resource**: `Attendance` object

**Access Rules**:
- **Admin**: ✅ Full access
- **Head Teacher**: ✅ Can access attendance in their department
- **Teacher**: ✅ Can access attendance for their classes
- **Others**: ❌ Denied

**When to use**:
```csharp
await _authorizationService.AuthorizeAsync(
    resource: attendance,  // Specific attendance record or null
    permission: PermissionType.ManageAttendances
);
```

---

### ClassAuthorizationHandler
**Resource**: `Class` object

**Access Rules**:
- **Admin**: ✅ Full access
- **Head Teacher**: ✅ Can manage classes in their department
- **Teacher**: ✅ Can only view their assigned classes
- **Others**: ❌ Denied

**When to use**:
```csharp
await _authorizationService.AuthorizeAsync(
    resource: classObj,  // Specific class or null for global operations
    permission: PermissionType.EditClasses
);
```

---

### EmployeeAuthorizationHandler
**Resource**: `Employee` object

**Access Rules**:
- **Admin**: ✅ Full access
- **Head Teacher**: ✅ Can view employees in their department (resource: null)
- **Teacher**: ❌ Denied
- **Others**: ❌ Denied

**When to use**:
```csharp
// View all employees in department (resource: null)
await _authorizationService.AuthorizeAsync(
    resource: null,
    permission: PermissionType.ManageEmployees
);

// Access specific employee (resource: employee)
await _authorizationService.AuthorizeAsync(
    resource: employee,
    permission: PermissionType.ManageEmployees
);
```

---

### CandidateAuthorizationHandler
**Resource**: `Candidate` object

**Access Rules**:
- **Admin**: ✅ Full access
- **All others**: ❌ Denied (admin-only resource)

**When to use**:
```csharp
// Candidates are admin-only - typically no resource check needed
await _authorizationService.AuthorizeAsync(
    resource: null,
    permission: PermissionType.ManageCandidates
);
```

---

### GradeAuthorizationHandler
**Resource**: `Grade` object

**Access Rules**:
- **Admin**: ✅ Full access
- **Head Teacher**: ✅ Can view (resource: null)
- **Teacher**: ✅ Can view (resource: null)
- **With resource object**: ❌ Only admins

**When to use**:
```csharp
// Teachers can view grades (no specific resource)
await _authorizationService.AuthorizeAsync(
    resource: null,  // Always pass null - grade is system-level
    permission: PermissionType.ViewGrades  // (if permission exists)
);
```

**Note**: Grade is a system resource without department hierarchy.

---

### ExamAuthorizationHandler
**Resource**: `Exam` object

**Access Rules**:
- **Admin**: ✅ Full access
- **Head Teacher**: ✅ Can view (resource: null)
- **Teacher**: ✅ Can view (resource: null)
- **With resource object**: ❌ Only admins

**When to use**:
```csharp
// Teachers can view exams (no specific resource)
await _authorizationService.AuthorizeAsync(
    resource: null,  // Always pass null - exam is system-level
    permission: PermissionType.ViewExams  // (if permission exists)
);
```

**Note**: Exam is a system resource without department hierarchy.

---

### ScoreAuthorizationHandler
**Resource**: `Score` object

**Access Rules**:
- **Admin**: ✅ Full access
- **Head Teacher**: ✅ Can manage scores in their department
- **Teacher**: ✅ Can manage scores for students in their classes
- **Others**: ❌ Denied

**When to use**:
```csharp
// Check if teacher can edit student score
await _authorizationService.AuthorizeAsync(
    resource: score,
    permission: PermissionType.EditStudents  // Using student permission as proxy
);
```

---

## Integration Checklist

- [ ] All handlers created and registered
- [ ] Build successful (no compilation errors)
- [ ] Update all service layer methods to check authorization
- [ ] Update all ViewModels to check authorization before operations
- [ ] Update XAML buttons/menus to bind to permission flags
- [ ] Test each module with different roles (Admin, Head Teacher, Teacher)
- [ ] Test resource-level authorization (user can only access their department/class)
- [ ] Add unit tests for each handler
- [ ] Document permission matrix for your team

---

## Permission Matrix Reference

```
┌──────────┬──────────┬──────────┬─────────┬──────────────┐
│ Resource │  Admin   │ HeadTeach│ Teacher │ Others       │
├──────────┼──────────┼──────────┼─────────┼──────────────┤
│ Student  │ Full ✅  │ Dept 🔒  │ Class 🔒│ Denied ❌    │
│Attendance│ Full ✅  │ Dept 🔒  │ Class 🔒│ Denied ❌    │
│ Class    │ Full ✅  │ Dept 🔒  │ View 👁 │ Denied ❌    │
│ Employee │ Full ✅  │ Dept 👁  │ Denied ❌ Denied ❌   │
│Candidate │ Full ✅  │ Denied ❌│ Denied ❌ Denied ❌   │
│ Grade    │ Full ✅  │ View 👁  │ View 👁 │ Denied ❌    │
│ Exam     │ Full ✅  │ View 👁  │ View 👁 │ Denied ❌    │
│ Score    │ Full ✅  │ Dept 🔒  │ Class 🔒│ Denied ❌    │
└──────────┴──────────┴──────────┴─────────┴──────────────┘

Legend:
Full ✅ = All permissions granted
Dept 🔒 = Scoped to their department
Class 🔒 = Scoped to their class
View 👁 = Read-only access
Denied ❌ = No access
```

---

## Testing Your Authorization

### Manual Testing

1. **Log in as Admin**
   - ✅ All operations should be enabled
   - ✅ All data should be visible

2. **Log in as Head Teacher**
   - ✅ Can see students/attendance in their department only
   - ✅ Cannot see other departments' data
   - ✅ Cannot manage employees

3. **Log in as Teacher**
   - ✅ Can see students/attendance in their classes only
   - ✅ Cannot see other classes' data
   - ✅ Cannot manage employees
   - ✅ Cannot manage classes (can only view)

4. **Log in as other roles**
   - ❌ Should see "Access Denied" messages
   - ❌ Buttons/menus should be disabled

---

## Next Steps

1. ✅ Authorization handlers are implemented
2. ⬜ Add more specific permissions to `PermissionType` enum if needed
3. ⬜ Update database seeding with role-permission mappings
4. ⬜ Create comprehensive unit tests for all handlers
5. ⬜ Document in your team wiki/repository

