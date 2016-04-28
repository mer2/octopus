namespace Octopus.SecurityPermissions
{
	/// <summary>
	/// 权限实体接口
	/// </summary>
	public interface IPermissionObject
	{
		/// <summary>
		/// 资源，即权限编号
		/// </summary>
		string PermissionNo { get; }
		/// <summary>
		/// 权限值（可选）
		/// </summary>
		string PermissionValue { get; }
		/// <summary>
		/// 权限所属应用
		/// </summary>
		string AppNo { get; }
		/// <summary>
		/// 是否有效
		/// </summary>
		bool Enabled { get; }
	}
}
