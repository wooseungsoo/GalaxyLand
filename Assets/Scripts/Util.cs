using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public  static class Util
{
    public static int[] CalTime(int time) //초를 시간을 계산
    {
        int second=0;
        int minute=0;
        int hour = 0;

        minute = (int)time / 60;
        hour = (int)minute / 60;
        minute = minute % 60;
        second = (int)time % 60;

        return new int[] { hour, minute, second };
    }

}
