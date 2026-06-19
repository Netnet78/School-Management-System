// ============================================================================
// AUTHORIZATION HANDLERS - PRACTICAL CODE EXAMPLES
// ============================================================================
// Copy and adapt these examples to your specific use cases
// ============================================================================

// ============================================================================
// EXAMPLE 1: ViewModel with Authorization Checks
// ============================================================================

/*
public partial class ClassViewModel : ObservableObject, IViewModel
{
    private readonly IClassService _classService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IMessageService _messageService;
    private readonly IDispatcherService _dispatcherService;

    [ObservableProperty]
    private ObservableCollection<Class> classes = new();

    [ObservableProperty]
    private bool canCreateClass = false;

    [ObservableProperty]
    private bool canEditClass = false;

    [ObservableProperty]
    private bool canDeleteClass = false;

    [ObservableProperty]
    private Class? selectedClass;

    public ClassViewModel(
        IClassService classService,
        IAuthorizationService authorizationService,
        IMessageService messageService,
        IDispatcherService dispatcherService)
    {
        _classService = classService;
        _authorizationService = authorizationService;
        _messageService = messageService;
        _dispatcherService = dispatcherService;
    }

    // Load permissions and data
    public async Task LoadAsync()
    {
        await CheckPermissionsAsync();
        await LoadClassesAsync();
    }

    // Check what this user can do
    private async Task CheckPermissionsAsync()
    {
        CanCreateClass = await _authorizationService.AuthorizeAsync(
            null,
            PermissionType.InsertClasses
        );

        CanEditClass = await _authorizationService.AuthorizeAsync(
            null,
            PermissionType.EditClasses
        );

        CanDeleteClass = await _authorizationService.AuthorizeAsync(
            null,
            PermissionType.DeleteClasses
        );
    }

    // Load classes with authorization
    private async Task LoadClassesAsync()
    {
        bool authorized = await _authorizationService.AuthorizeAsync(
            null,
            PermissionType.ViewClasses
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

        var response = await _classService.GetAllAsync();
        if (response.Status != Status.Success)
        {
            _messageService.Show(response.Message, "Error", icon: MessageIcon.Error);
            return;
        }

        await _dispatcherService.InvokeAsync(() =>
        {
            Classes = new ObservableCollection<Class>(response.Value ?? []);
        });
    }

    // Create class with authorization
    [RelayCommand]
    private async Task CreateClassAsync()
    {
        bool authorized = await _authorizationService.AuthorizeAsync(
            null,
            PermissionType.InsertClasses
        );

        if (!authorized)
        {
            _messageService.Show(
                "You don't have permission to create classes",
                "Access Denied",
                icon: MessageIcon.Hand
            );
            return;
        }

        var dialog = new CreateClassDialog();
        if (dialog.ShowDialog() == true)
        {
            var response = await _classService.AddAsync(dialog.NewClass);
            // ... handle response
        }
    }

    // Edit specific class with resource-level authorization
    [RelayCommand]
    private async Task EditClassAsync()
    {
        if (SelectedClass == null) return;

        bool authorized = await _authorizationService.AuthorizeAsync(
            resource: SelectedClass,
            permission: PermissionType.EditClasses
        );

        if (!authorized)
        {
            _messageService.Show(
                "You don't have permission to edit this class",
                "Access Denied",
                icon: MessageIcon.Hand
            );
            return;
        }

        // Open edit dialog
        var dialog = new EditClassDialog(SelectedClass);
        if (dialog.ShowDialog() == true)
        {
            var response = await _classService.UpdateAsync(dialog.UpdatedClass);
            // ... handle response
        }
    }

    // Delete specific class with resource-level authorization
    [RelayCommand]
    private async Task DeleteClassAsync()
    {
        if (SelectedClass == null) return;

        bool authorized = await _authorizationService.AuthorizeAsync(
            resource: SelectedClass,
            permission: PermissionType.DeleteClasses
        );

        if (!authorized)
        {
            _messageService.Show(
                "You don't have permission to delete this class",
                "Access Denied",
                icon: MessageIcon.Hand
            );
            return;
        }

        MessageResult confirm = _messageService.Show(
            $"Are you sure you want to delete class '{SelectedClass.Name}'?",
            "Confirm Delete",
            MessageButton.YesNo,
            MessageIcon.Question
        );

        if (confirm == MessageResult.Yes)
        {
            var response = await _classService.DeleteAsync(SelectedClass);
            // ... handle response
            await LoadClassesAsync();
        }
    }
}
*/

// ============================================================================
// EXAMPLE 2: XAML Binding to Permission Properties
// ============================================================================

/*
<UserControl x:Class="SchoolManagement.Presentation.Views.ClassView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <StackPanel>
        <!-- Header with Permission-Based Buttons -->
        <StackPanel Orientation="Horizontal" Margin="0,0,0,20">
            <TextBlock Text="Classes" FontSize="24" FontWeight="Bold"/>

            <!-- Create Button - Only visible/enabled if user has permission -->
            <Button Command="{Binding CreateClassCommand}"
                    IsEnabled="{Binding CanCreateClass}"
                    Margin="10,0,0,0">
                <StackPanel Orientation="Horizontal">
                    <TextBlock Text="Create Class"/>
                </StackPanel>
            </Button>
        </StackPanel>

        <!-- Classes List -->
        <DataGrid ItemsSource="{Binding Classes}" 
                  SelectedItem="{Binding SelectedClass}">
            <DataGrid.Columns>
                <DataGridTextColumn Header="Name" Binding="{Binding Name}"/>
                <DataGridTextColumn Header="Grade" Binding="{Binding Grade.Name}"/>

                <!-- Edit Button - Only visible/enabled if user has permission -->
                <DataGridTemplateColumn Header="Actions">
                    <DataGridTemplateColumn.CellTemplate>
                        <DataTemplate>
                            <Button Command="{Binding DataContext.EditClassCommand, 
                                    RelativeSource={RelativeSource AncestorType=DataGrid}}"
                                    IsEnabled="{Binding DataContext.CanEditClass,
                                    RelativeSource={RelativeSource AncestorType=DataGrid}}"
                                    Content="Edit"/>
                        </DataTemplate>
                    </DataGridTemplateColumn.CellTemplate>
                </DataGridTemplateColumn>

                <!-- Delete Button - Only visible/enabled if user has permission -->
                <DataGridTemplateColumn Header="">
                    <DataGridTemplateColumn.CellTemplate>
                        <DataTemplate>
                            <Button Command="{Binding DataContext.DeleteClassCommand,
                                    RelativeSource={RelativeSource AncestorType=DataGrid}}"
                                    IsEnabled="{Binding DataContext.CanDeleteClass,
                                    RelativeSource={RelativeSource AncestorType=DataGrid}}"
                                    Content="Delete"
                                    Foreground="Red"/>
                        </DataTemplate>
                    </DataGridTemplateColumn.CellTemplate>
                </DataGridTemplateColumn>
            </DataGrid.Columns>
        </DataGrid>
    </StackPanel>

</UserControl>
*/

// ============================================================================
// EXAMPLE 3: Service Layer Authorization
// ============================================================================

/*
public class StudentService : CrudServiceBase<Student>
{
    private readonly IStudentRepository _repository;
    private readonly IAuthorizationService _authorizationService;

    public StudentService(
        IStudentRepository repository,
        IAuthorizationService authorizationService)
        : base(repository)
    {
        _repository = repository;
        _authorizationService = authorizationService;
    }

    // Override delete to add authorization
    public async Task<ReturnResponse> DeleteStudentAsync(Student student)
    {
        // Check authorization before allowing deletion
        bool authorized = await _authorizationService.AuthorizeAsync(
            resource: student,
            permission: PermissionType.DeleteStudents
        );

        if (!authorized)
        {
            return new ReturnResponse
            {
                Status = Status.Failed,
                Message = "You don't have permission to delete this student"
            };
        }

        try
        {
            await _repository.DeleteAsync(student);
            return new ReturnResponse { Status = Status.Success };
        }
        catch (Exception ex)
        {
            return new ReturnResponse
            {
                Status = Status.Failed,
                Message = ex.Message
            };
        }
    }

    // Override update to add authorization
    public async Task<ReturnResponse> UpdateStudentAsync(Student student)
    {
        // Check authorization before allowing update
        bool authorized = await _authorizationService.AuthorizeAsync(
            resource: student,
            permission: PermissionType.EditStudents
        );

        if (!authorized)
        {
            return new ReturnResponse
            {
                Status = Status.Failed,
                Message = "You don't have permission to edit this student"
            };
        }

        try
        {
            await _repository.UpdateAsync(student);
            return new ReturnResponse { Status = Status.Success };
        }
        catch (Exception ex)
        {
            return new ReturnResponse
            {
                Status = Status.Failed,
                Message = ex.Message
            };
        }
    }
}
*/

// ============================================================================
// EXAMPLE 4: Global vs Resource-Specific Authorization
// ============================================================================

/*
// GLOBAL AUTHORIZATION - No specific resource
// Use this to check if user can perform an action in general
private async Task LoadStudentsAsync()
{
    bool canView = await _authorizationService.AuthorizeAsync(
        resource: null,  // ← No specific student
        permission: PermissionType.ViewStudents
    );

    if (!canView) return;
    // Load and display all students user has access to
}

// RESOURCE-SPECIFIC AUTHORIZATION
// Use this to check if user can perform an action on THIS SPECIFIC resource
private async Task DeleteStudentAsync(Student student)
{
    bool canDelete = await _authorizationService.AuthorizeAsync(
        resource: student,  // ← This specific student
        permission: PermissionType.DeleteStudents
    );

    if (!canDelete)
    {
        // This student might be in a different department/class
        // that the user cannot access
        return;
    }
    // Delete this specific student
}
*/

// ============================================================================
// EXAMPLE 5: Multiple Permissions (AND/OR)
// ============================================================================

/*
// User must have BOTH permissions (OperatorMode.AND)
bool canManageScores = await _authorizationService.AuthorizeAsync(
    null,
    OperatorMode.AND,
    PermissionType.ViewStudents,
    PermissionType.EditStudents
);

// User must have AT LEAST ONE permission (OperatorMode.OR)
bool canViewStudentInfo = await _authorizationService.AuthorizeAsync(
    null,
    OperatorMode.OR,
    PermissionType.ViewStudents,
    PermissionType.EditStudents,
    PermissionType.DeleteStudents
);
*/

// ============================================================================
// EXAMPLE 6: Authorization in Presentation.Shared (Reusable)
// ============================================================================

/*
// Create a reusable authorization helper in Presentation.Shared

namespace SchoolManagement.Presentation.Shared.Extensions
{
    public static class AuthorizationExtensions
    {
        /// <summary>
        /// Check if user can perform an action and show error if not
        /// </summary>
        public static async Task<bool> CheckAuthorizationAsync(
            this IAuthorizationService authorizationService,
            IMessageService messageService,
            object? resource,
            PermissionType permission,
            string errorTitle = "Access Denied")
        {
            bool authorized = await authorizationService.AuthorizeAsync(
                resource,
                permission
            );

            if (!authorized)
            {
                messageService.Show(
                    "You don't have permission to perform this action",
                    errorTitle,
                    icon: MessageIcon.Hand
                );
            }

            return authorized;
        }

        /// <summary>
        /// Safely execute an action only if authorized
        /// </summary>
        public static async Task<bool> ExecuteIfAuthorizedAsync(
            this IAuthorizationService authorizationService,
            IMessageService messageService,
            object? resource,
            PermissionType permission,
            Func<Task> action)
        {
            if (!await CheckAuthorizationAsync(
                authorizationService,
                messageService,
                resource,
                permission))
            {
                return false;
            }

            await action();
            return true;
        }
    }
}

// Usage:
await _authorizationService.ExecuteIfAuthorizedAsync(
    _messageService,
    student,
    PermissionType.DeleteStudents,
    async () =>
    {
        await _studentService.DeleteAsync(student);
        await LoadStudentsAsync();
    }
);
*/

// ============================================================================
// EXAMPLE 7: Unit Tests for Authorization Handlers
// ============================================================================

/*
[TestClass]
public class ClassAuthorizationHandlerTests
{
    private ClassAuthorizationHandler _handler;
    private Mock<IAuthorizationService> _authServiceMock;

    [TestInitialize]
    public void Setup()
    {
        _handler = new ClassAuthorizationHandler();
        _authServiceMock = new Mock<IAuthorizationService>();
    }

    [TestMethod]
    public async Task Admin_Should_Have_Full_Access()
    {
        // Arrange
        var adminUser = new User
        {
            Role = new Role { Name = RoleType.Admin.ToString() }
        };
        var testClass = new Class { Name = "Math 101" };

        // Act
        var result = await _handler.HandleAsync(
            adminUser,
            testClass,
            OperatorMode.AND,
            PermissionType.EditClasses
        );

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task HeadTeacher_Can_Access_Own_Department_Class()
    {
        // Arrange
        var headTeacher = new User
        {
            Role = new Role
            {
                Name = RoleType.HeadTeacher.ToString(),
                Permissions = new List<Permission>
                {
                    new Permission { Name = PermissionType.EditClasses.ToString() }
                }
            },
            Employee = new Employee { DepartmentId = 1 }
        };

        var @class = new Class
        {
            Generation = new Generation { DepartmentId = 1 }
        };

        // Act
        var result = await _handler.HandleAsync(
            headTeacher,
            @class,
            OperatorMode.AND,
            PermissionType.EditClasses
        );

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task HeadTeacher_Cannot_Access_Other_Department_Class()
    {
        // Arrange
        var headTeacher = new User
        {
            Role = new Role
            {
                Name = RoleType.HeadTeacher.ToString(),
                Permissions = new List<Permission>
                {
                    new Permission { Name = PermissionType.EditClasses.ToString() }
                }
            },
            Employee = new Employee { DepartmentId = 1 }
        };

        var @class = new Class
        {
            Generation = new Generation { DepartmentId = 2 }  // Different department
        };

        // Act
        var result = await _handler.HandleAsync(
            headTeacher,
            @class,
            OperatorMode.AND,
            PermissionType.EditClasses
        );

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task Teacher_Can_Only_View_Own_Classes()
    {
        // Arrange
        var teacherClass = new Class { Id = 101, Name = "Science 101" };
        var teacher = new User
        {
            Role = new Role
            {
                Name = RoleType.Teacher.ToString(),
                Permissions = new List<Permission>
                {
                    new Permission { Name = PermissionType.ViewClasses.ToString() }
                }
            },
            Employee = new Employee
            {
                Classes = new List<Class> { teacherClass }
            }
        };

        // Act - teacher views their own class
        var result = await _handler.HandleAsync(
            teacher,
            teacherClass,
            OperatorMode.AND,
            PermissionType.ViewClasses
        );

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task Teacher_Cannot_Edit_Classes()
    {
        // Arrange
        var teacher = new User
        {
            Role = new Role
            {
                Name = RoleType.Teacher.ToString(),
                Permissions = new List<Permission>
                {
                    new Permission { Name = PermissionType.ViewClasses.ToString() }
                }
            },
            Employee = new Employee { Classes = new List<Class>() }
        };

        // Act
        var result = await _handler.HandleAsync(
            teacher,
            null,
            OperatorMode.AND,
            PermissionType.EditClasses
        );

        // Assert
        Assert.IsFalse(result);
    }
}
*/
