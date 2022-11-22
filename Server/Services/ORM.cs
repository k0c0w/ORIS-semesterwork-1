using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using DateTime = System.DateTime;

namespace Server.Services.ServerServices;

public class ORM
{
    public string _connectionString { get; }

    public ORM(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IEnumerable<T> Select<T>(WhereModel<T> Condition) => Select<T>(Condition.GetSQLConstraints);

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
        var propeties = modelType.GetProperties();
        var sb = new StringBuilder();
        var l = $"insert into {GetTableName(modelType)} (";
        sb.Append(l);
        sb.Append(JoinParameters(propeties.Select(x => x.Name).Where(x => x != "Id")));
        sb.Append(") values( ");
        sb.Append(string.Join(',', GetConvertedProperties(obj, propeties.Where(x => x.Name != "Id"))));
        sb.Append(')');

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
            var type = property.PropertyType;
            if (type is bool)
                converted.Add((bool)property.GetValue(instance) == true ? 1 : 0);
            else if (type is DateTime)
            {
                var dateTime = (DateTime)property.GetValue(instance);
                converted.Add(dateTime.ToString("'MM/dd/yyyy hh:mm:ss'"));
            }
            else if (type is DateOnly)
            {
                var date = (DateOnly)property.GetValue(instance);
                converted.Add(date.ToString("'MM/dd/yyyy'"));
            }
            else
                converted.Add($"'{property.GetValue(instance)}'");
        }
        return converted;
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