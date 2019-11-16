using UnityEngine;

public static class DelegateUtility
{

    /// <summary>
    /// Indicates if a method with name, methodName is subscribed to a delegate, del, on object, invokingObject
    /// </summary>
    public static bool isSubscribed(this System.Delegate del, object invokingObject, string methodName)
    {
        bool isSubscribed = false;

        if (del != null) //Delegate has methods subscribed
        {
            System.Delegate[] invocationList = del.GetInvocationList();
            int i = 0;
            while (i < invocationList.Length && !isSubscribed)
            {
                System.Delegate subscribedDelegate = invocationList[i];
                if (del.Method.Name == methodName && subscribedDelegate.Target.Equals(invokingObject))
                {
                    isSubscribed = true;
                }

                i++;
            }
        }

        return isSubscribed;
    }

    /// <summary>
    /// Indicates if a method with name, methodName is subscribed to a delegate, del (meant for static methods)
    /// </summary>
    public static bool isSubscribed(this System.Delegate del, string methodName)
    {
        bool isSubscribed = false;

        if (del != null) //Delegate has methods subscribed
        {
            System.Delegate[] invocationList = del.GetInvocationList();
            int i = 0;
            while (i < invocationList.Length && !isSubscribed)
            {
                System.Delegate subscribedDelegate = invocationList[i];
                if (del.Method.Name == methodName)
                {
                    isSubscribed = true;
                }

                i++;
            }
        }

        return isSubscribed;
    }

}
