using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Errors
{
    public static Dictionary<string, string> dicRus = new Dictionary<string, string>() {
        {"Cannot connect to destination host", "Включите интернет на устройстве или подключитесь к WIFI. Не удается подключиться к удаленному хосту."},
        {"JSON parse error: Invalid value.", "Не удалось обработать ответ от сервера."},
        {"Unable to load from url", "Невозможно загрузить данные." },
        {"No Internet Connection", "Включите интернет на устройстве или подключитесь к WIFI. Нет соединения с интернетом."}
    };

    public static Dictionary<string, Type> dic = new Dictionary<string, Type>() {
        {"unable to load from url", typeof(NetworkConnectionException) },
        {"failed because dependent operation failed", typeof(DependentOperationFailedException) }
    };

    public static Type GetTypeException(string condition)
    {
        foreach (var item in dic)
        {
            if (condition.Contains(item.Key))
            {
                return item.Value;
            }
        }
        return typeof(UnknownException);
    }

}

public class NetworkConnectionException : Exception
{
    public override string Message => "Невозможно загрузить данные.";
}

public class UnknownException : Exception {
    public override string Message => "Неизвестная ошибка.";
}

public class DependentOperationFailedException : Exception
{
    public override string Message => "Сбой зависимой операции.";
}

public class OnAddressableNullResultException : Exception
{
    public override string Message => "Результат пуст.";
}
