namespace MerchantDeviceManager.Domain.Entities;

/// <summary>
/// Operator role for RBAC. Admin: full access; Support: create/view; Viewer: view only.
/// </summary>
public enum OperatorRole
{
    Admin,
    Support,
    Viewer
}
