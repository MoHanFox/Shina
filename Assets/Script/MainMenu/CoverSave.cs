using OfficeOpenXml;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CoverSave: MonoBehaviour
{

    public void DoCover()
    {
        MessageBox.Show("本次操作将会覆盖存档，其他存档不受影响。不过，你真的要这么做吗？", "你在扼杀你的故事……", DialogResult.OkCancel);
        MessageBox.okText = "是";
        MessageBox.cancelText = "否";
        MessageBox.clickOk = () => {
            SaveManager.data0 = SaveManager.inti.CreateSave();

            Audiobook.setupChapter(SaveManager.data0.chapter);

            GlobalHook.UpdateAuidoBookO();

            Audiobook.startPlaying = true;
        };
        MessageBox.clickCancel = () =>
        {

        };
        UiMessage.open();
    }

}
