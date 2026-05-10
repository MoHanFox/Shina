using OfficeOpenXml;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GlobalHook : MonoBehaviour
{
    public GameObject MesBox;
    public GameObject NotiBox;
    public GameObject AuidoBookO;
    public static bool needOpenMes = false;
    public static bool needOpenNoti = false;
    public static bool updateAuidoBookO = false;

    public static bool bookToRow = false;
    public static int bookToRowInt = 0;


    // Update is called once per frame
    void Update()
    {
        if (needOpenMes)
        {
            needOpenMes = false;
            MesBox.SetActive(true);
        }

        if (needOpenNoti)
        {
            needOpenNoti = false;
            if(NotiBox.activeInHierarchy)
                NotiBox.SetActive(false);
            NotiBox.SetActive(true);
        }

        if (updateAuidoBookO)
        {
            updateAuidoBookO = false;

            Audiobook.setupChapter(SaveManager.data0.chapter);

            if(Audiobook.startPlaying)
                AuidoBookO.GetComponent<Audiobook>().OnEnable();
        }
    }

    public static void openMessageBox()
    {
        needOpenMes = true;
    }
    public static void openNotiBox()
    {
        needOpenNoti = true;
    }
    public static void UpdateAuidoBookO()
    {
        updateAuidoBookO = true;
    }

    public static void BookToRow(int target)
    {
        bookToRow = true;
        bookToRowInt = target;
    }
}
