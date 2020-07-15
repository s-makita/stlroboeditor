using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Resources;
using System.IO;
using System.Text.RegularExpressions;

namespace StellarRobo
{
    class MineTypeData
    {
        //定数宣言
        private const string CONST_MINE_TYPE_FILE = "minetype";         //追加Mine/Typeファイル名用

        private static List<string> mine_type_list = new List<string>
        {
            "application" ,
            "audio" ,
            "font" ,
            "image" ,
            "message" ,
            "model" ,
            "multipart" ,
            "text" ,
            "video" ,
            "other",
        };      //Mine/Type一覧ファイル用

        public static string GetMineType(ResourceManager resourceManager,string appPath)
        {
            //変数宣言
            string mineType = string.Empty;

            //リソースに登録されているMine/Typeを読み込む
            foreach(string resourceName in mine_type_list)
            {
                mineType += resourceManager.GetString(resourceName);
            }

            //追加のMine/Typeはあるか？
            string file_name = Path.Combine(appPath, CONST_MINE_TYPE_FILE);
            if (File.Exists(file_name))
            {
                //追加のMine/Typeを取得
                using (FileStream fileStream = new FileStream(file_name, FileMode.Open))
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    //読み込み
                    string buffer = streamReader.ReadToEnd();
                    mineType += buffer;
                }
            }

            //改行を「,」に変更する
            mineType = Regex.Replace(mineType, "\r\n", ",");

            //戻り値設定
            return mineType;
        }
    }
}
