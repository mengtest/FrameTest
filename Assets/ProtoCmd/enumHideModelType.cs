using UnityEngine;
using System.Collections;

public class enumHideModelType
{
    public const int HIDE_NONE = 0;
    public const int HIDE_HORSE = 1;
    public const int HIDE_WING = 2;
    public const int HIDE_FABAO = 4;
    public const int HIDE_WEAPON = 8;

    static public bool Check(int type, int checkType)
    {
        return (type & checkType) > 0;
    }
}
