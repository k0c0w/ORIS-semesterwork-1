using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using DateTime = System.DateTime;

namespace Server.Services.ServerServices;

public class ORM
{
    private static readonly string _connectionStringFromJson;
    public string _connectionString { get; }

    static ORM()
    {
        _connectionStringFromJson =
            ServerSettings.GetServerSettingsFromFile(new ConsoleLogger()).DatabaseConnectionString;
    }
    
    public ORM(string connectionString="")
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            if (string.IsNullOrEmpty(_connectionStringFromJson))
                throw new ArgumentException("Connection string must be assigned before you use ORM.");
            _connectionString = _connectionStringFromJson;
        }
        else
        {
            _connectionString = connectionString;
        }
    }

    public IEnumerable<T> Select<T>(WhereModel<T> condition) => Select<T>(condition.GetSQLConstraints);

    public IEnumerable<T> Select<T>(IEnumerable<WhereModel<T>> models) 
        => Select<T>(CreateWhereCondition(models));

    public IEnumerable<T> Select<T>()
    {
        return Select<T>("");
    }

    private IEnumerable<T> Select<T>(string whereCondition)
    {
        if (!string.IsNullOrEmpty(whereCondition))
            whereCondition = " WHERE " + whereCondition;
        var list = new List<T>();
        var modelType = typeof(T);
        var properties = modelType.GetProperties();
        using var connection = new SqlConnection(_connectionString);
        using var cursor = connection.CreateCommand();
        cursor.CommandText = $"select * from {GetTableName(modelType)}{whereCondition}";
        connection.Open();
        var reader = cursor.ExecuteReader();
        while (reader.Read())
        {
            var obj = Activator.CreateInstance<T>();
            foreach (var property in properties)
            {
                var read = reader[property.Name];
                if(read is DBNull)
                    continue;
                property.SetValue(obj, read);
            }
            list.Add(obj);
        }

        return list;
    }

    public int Insert<T>(T obj)
    {
        var modelType = obj.GetType();
        var properties = modelType.GetProperties().Where(x => x.Name != "Id");
        var sb = new StringBuilder();
        var l = $"insert into {GetTableName(modelType)} (";
        sb.Append(l);
        sb.Append(JoinParameters(properties.Select(x => x.Name)));
        sb.Append(") values( ");
        sb.Append(string.Join(',', GetConvertedProperties(obj, properties)));
        sb.Append(')');

        return ExecuteNonQuery(sb.ToString());
    }

    public int Update<T>(T updated, WhereModel<T> condition)
    {
        var where = condition.GetSQLConstraints;
        var modelType = updated.GetType();
        var properties = modelType.GetProperties().Where(x => x.Name != "Id");
        var iterated = false;
        var sb = new StringBuilder();
        sb.Append($"UPDATE {GetTableName(modelType)} SET ");
        foreach (var property in properties)
        {
            var value = property.GetValue(updated);
            if(value == null)
                continue;
            
            sb.Append($"{property.Name} = {GetConvertedToSqlString(value)}");
            sb.Append(", ");
            iterated = true;
        }

        if (iterated)
            sb.Remove(sb.Length - 2, 2);
        
        if (!string.IsNullOrEmpty(where))
            sb.Append($" WHERE {where}");
        return ExecuteNonQuery(sb.ToString());
    }

    public int Delete<T>(IEnumerable<WhereModel<T>> models)
    {
        var whereCondition = CreateWhereCondition(models);
        if (!string.IsNullOrEmpty(whereCondition))
            whereCondition = " WHERE " + whereCondition;
        var sql = "DELETE FROM {GetTableName(typeof(T))}{whereCondition}";
        return ExecuteNonQuery(sql);
    }

    private static string CreateWhereCondition<T>(IEnumerable<WhereModel<T>> models)
        => string.Join(" OR ", models.Select(x => $"({x.GetSQLConstraints})"));
    
    private int ExecuteNonQuery(string command)
    {
        var affectedRows = 0;
        using var connection = new SqlConnection(_connectionString);
        using var cursor = connection.CreateCommand();
        cursor.CommandText = command;
        connection.Open();
        affectedRows = cursor.ExecuteNonQuery();

        return affectedRows;
    }
    
    private static object? GetConvertedProperty(object instance, PropertyInfo property)
        => GetConvertedProperties(instance, new[] { property }).FirstOrDefault();

    private static IEnumerable<object> GetConvertedProperties(object instance, IEnumerable<PropertyInfo> properties)
    {
        var converted = new List<object>();
        foreach(var property in properties)
        {
            converted.Add(GetConvertedToSqlString(property.GetValue(instance)));
        }
        return converted;
    }

    private static string GetConvertedToSqlString(object? value)
    {
        if (value == null)
            return "NULL";
        
        if (value is bool boolean)
            return boolean ? "1" : "0";
        else if (value is DateTime dateTime)
        {
            return string.Format("'{0}'", dateTime.ToString("dd/MM/yyyy").Replace('.', '/'));
        }
        return $"'{value}'";
    }

    private static string JoinParameters<T>(IEnumerable<T> parameters) => string.Join(',', parameters);
    
    private static string GetTableName(Type modelType)
    {
        var name = modelType.GetCustomAttribute<TableAttribute>()?.Name;
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"No data about models table {modelType.Name}");
        return name;
    }
}

public class WhereModel<TModel>
{
    private readonly string _SQLContraints = "";
    
    public WhereModel(TModel model)
    {
        var props = model.GetType()
                                              .GetProperties()
                                              .Where(p => p.GetValue(model) != null)
                                              .Select(p => $"{p.Name}='{p.GetValue(model)}'");
        _SQLContraints = string.Join(" AND ", props);
    }

    public string GetSQLConstraints => _SQLContraints;
}