using OfficeOpenXml;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class RePlayChapter: MonoBehaviour
{
    public string chapter;
    public string progress_tag = null;
    public void DoCover()
    {
        MessageBox.Show("您将从这篇章节开始游玩，此外，你在这一章节的行为就将会被重新记录！", "时光倒流……", DialogResult.OkCancel);
        MessageBox.okText = "游玩";
        MessageBox.cancelText = "取消";
        MessageBox.clickOk = () => {
            Save reSave = SaveManager.inti.CreateSave();
            reSave.chapter = chapter;
            reSave.progress = progress_tag.IsUnityNull() ? 1 : Audiobook.搜寻Tag行(progress_tag);
            reSave.事件标签 = SaveManager.data0.事件标签;

            SaveManager.data0 = reSave;

            Audiobook.setupChapter(chapter);

            GlobalHook.UpdateAuidoBookO();

            Audiobook.startPlaying = true;
        };
        MessageBox.clickCancel = () =>
        {

        };
        UiMessage.open();
    }

}
