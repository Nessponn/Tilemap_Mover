using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class Tilemap_Mover : MonoBehaviour
{
    //ゲームオブジェクト
    public GameObject StageGrid;//タイルマップ本体
    public GameObject[] StageMaps;//追加したいマップデータ
    public GameObject Image_Tilemap;//背景用タイルマップ
    public TileBase a;

    //変数
    private float t = 0;//時間
    [Range(1,6)]public int MAP_SPEED = 4;//進むスピード（大きすぎるとシステムがバグるので、ゲーム的にも６ぐらいがちょうどいいかも）
    private int size_bottom = 35;//最低サイズ(マップ追加判定値)

    // Start is called before the first frame update
    void Start()
    {
        //実行中に小さいサイズから大きいサイズのマップに転換することは無理
        //最初にリサイズしておくことで擬似的にマップのリサイズが可能

        var tilemap = StageGrid.GetComponent<Tilemap>();
        tilemap.CompressBounds();

        int heightmax = tilemap.cellBounds.max.y;//本体のサイズで最初はキャッシュ
        int heightmin = tilemap.cellBounds.min.y;
        for (int i = 0; i < StageMaps.Length; i++)
        {
            StageMaps[i].GetComponent<Tilemap>().CompressBounds();
            heightmax = Mathf.Max(heightmax, StageMaps[i].GetComponent<Tilemap>().cellBounds.max.y);

            heightmin = Mathf.Min(heightmin, StageMaps[i].GetComponent<Tilemap>().cellBounds.min.y);
        }
        var list = new List<TileInfo>();

        var tile1 = tilemap.GetTile<Tile>(new Vector3Int(tilemap.cellBounds.min.x, heightmax, 0));
        var position1 = new Vector3Int(tilemap.cellBounds.min.x, heightmax, 0);
        Vector3 rotation1 = tilemap.GetTransformMatrix(position1).rotation.eulerAngles;//回転を取る
        var maxinfo = new TileInfo(position1, 0, rotation1, tile1, MAP_SPEED);
        list.Add(maxinfo);

        var tile2 = tilemap.GetTile<Tile>(new Vector3Int(tilemap.cellBounds.min.x, heightmin, 0));
        var position2 = new Vector3Int(tilemap.cellBounds.min.x, heightmin, 0);
        Vector3 rotation2 = tilemap.GetTransformMatrix(position2).rotation.eulerAngles;//回転を取る
        var mininfo = new TileInfo(position2, 0, rotation2, tile2, MAP_SPEED);
        list.Add(mininfo);

        foreach (var data in list)
        {
            var pos = data.m_position;
            var rot = data.m_rotation;

                tilemap.SetTile(pos, a);
                Matrix4x4 matrix = Matrix4x4.TRS(Vector3Int.zero, Quaternion.Euler(rot), Vector3.one);
                tilemap.SetTransformMatrix(pos, matrix);
            
            pos.x += 1;
        }

        tilemap.CompressBounds();

        //tilemap.size = new Vector3Int(tilemap.size.x, heightmax, tilemap.size.z);
        //Debug.Log("size" + heightmax);

        //足した分のサイズにリサイズする（このメソッドを使わないとリサイズしてくれない）
        //tilemap.ResizeBounds();

    }
    // Update is called once per frame
    void Update()
    {
        //タイルマップの処理
        {
            t -= Time.deltaTime * MAP_SPEED;

            if (t < -MAP_SPEED)
            {

                var tilemap = StageGrid.GetComponent<Tilemap>();
                var bound = StageGrid.GetComponent<Tilemap>().cellBounds;

                for (int i = 0; i < StageGrid.transform.childCount; i++)
                {
                    Vector3 pos = StageGrid.transform.GetChild(i).transform.position;

                    
                    StageGrid.transform.GetChild(i).transform.position = new Vector3(pos.x - MAP_SPEED, pos.y, pos.z);
                    if (pos.x <= bound.min.x)
                    {
                        //ある程度左端に行くとオブジェクト消えるようにする
                        StageGrid.transform.GetChild(i).gameObject.SetActive(false);
                    }
                }

                

                if (bound.max.x > size_bottom)
                {
                    PassTile();
                }
                else
                {
                    int num = UnityEngine.Random.Range(0, StageMaps.Length);
                    StageMaps[num].GetComponent<Tilemap>().CompressBounds();
                    AddPass_Tile(num);

                    tilemap.size = new Vector3Int(tilemap.size.x - MAP_SPEED, tilemap.size.y, tilemap.size.z);
                    tilemap.ResizeBounds();
                }
                t = 0;
            }
            StageGrid.transform.position = new Vector3(t, StageGrid.transform.position.y, StageGrid.transform.position.z);
            Image_Tilemap.transform.position = new Vector3(t, StageGrid.transform.position.y, StageGrid.transform.position.z);
        }
        //タイトル画面の入力
    }

    //もともとのタイルを保存するクラス
    private class TileInfo
    {
        public readonly Vector3Int m_position;
        public readonly Vector3 m_rotation;
        public readonly TileBase m_tile;

        public TileInfo(Vector3Int position, int addposition_x, Vector3 rotation, TileBase tile, int distance)
        {
            position.x -= distance;
            position.x += addposition_x;
            m_position = position;
            m_rotation = rotation;
            m_tile = tile;
        }
    }

    public void PassTile()
    {
        //タイルの情報をずらす
        //0から-1担った瞬間、座標を0基準でタイル情報を左にずらす

        //タイルから何列目にいるか（y軸の値）の情報を引き出す
        int num = 0;
        //Vector3Int vec;
        var tilemap = StageGrid.GetComponent<Tilemap>();
        var bound = StageGrid.GetComponent<Tilemap>().cellBounds;

        int gridnum = bound.max.x;

        var list = new List<TileInfo>();

        for (int y = bound.max.y - 1; y >= bound.min.y; --y)//左上から右下にかけてタイルを代入していく
        {
            for (int x = bound.min.x; x < gridnum; ++x)
            {
                var tile = tilemap.GetTile<Tile>(new Vector3Int(x, y, 0));

                var position = new Vector3Int(x, y, 0);
                Vector3 rotation = tilemap.GetTransformMatrix(position).rotation.eulerAngles;//回転を取る

                //タイル情報の保存
                var info = new TileInfo(position, 0, rotation, tile, MAP_SPEED);
                list.Add(info);

                num++;
            }

            //タイル情報の適用
            foreach (var data in list)
            {
                var position = data.m_position;
                var rotation = data.m_rotation;

                if (position.x >= bound.min.x)
                {
                    tilemap.SetTile(position, data.m_tile);
                    Matrix4x4 matrix = Matrix4x4.TRS(Vector3Int.zero, Quaternion.Euler(rotation), Vector3.one);
                    tilemap.SetTransformMatrix(position, matrix);
                }
                position.x += 1;
            }
        }
        
        //if (MAP_SPEED > 1)
        //{
            tilemap.size = new Vector3Int(tilemap.size.x - MAP_SPEED, tilemap.size.y, tilemap.size.z);
            tilemap.ResizeBounds();
        //}
    }

    void AddPass_Tile(int num)
    {

        var tilemap = StageGrid.GetComponent<Tilemap>();
        var addtilemap = StageMaps[num].GetComponent<Tilemap>();

        var bound = StageGrid.GetComponent<Tilemap>().cellBounds;
        var add_bound = StageMaps[num].GetComponent<Tilemap>().cellBounds;

        //addtilemap.CompressBounds();

        /*
        tilemap.origin = addtilemap.origin;
        tilemap.size = new Vector3Int(tilemap.size.x + addtilemap.size.x, addtilemap.size.y, tilemap.size.z);
        tilemap.ResizeBounds();
        tilemap.CompressBounds();

        //追加するマップの横幅をもともとあるサイズと足す

        /*
        int heightmax = Mathf.Max(tilemap.size.y,addtilemap.size.y);

        tilemap.size = new Vector3Int(tilemap.size.x + addtilemap.size.x, heightmax, tilemap.size.z);
        Debug.Log("size"+heightmax);

        //足した分のサイズにリサイズする（このメソッドを使わないとリサイズしてくれない）
        tilemap.ResizeBounds();
        */

        tilemap.size = new Vector3Int(tilemap.size.x + addtilemap.size.x, tilemap.size.y, tilemap.size.z);
        tilemap.ResizeBounds();

        var list = new List<TileInfo>();

        for (int y = bound.max.y - 1; y >= bound.min.y; --y)//左上から右下にかけてタイルを代入していく
        {
            //もともと現存していたタイルマップの描画
            //上から順に、一列にタイルマップを描画しなおす

            for (int x = bound.min.x; x < bound.max.x; ++x)
            {
                var tile = tilemap.GetTile<Tile>(new Vector3Int(x, y, 0));

                var position = new Vector3Int(x, y, 0);
                Vector3 rotation = tilemap.GetTransformMatrix(position).rotation.eulerAngles;//回転を取る

                var info = new TileInfo(position, 0, rotation, tile, MAP_SPEED);
                list.Add(info);
            }

            for (int x = add_bound.min.x; x < add_bound.max.x; ++x)
            {
                var tile2 = addtilemap.GetTile<Tile>(new Vector3Int(x, y, 0));

                var position2 = new Vector3Int(x, y, 0);
                Vector3 rotation2 = addtilemap.GetTransformMatrix(position2).rotation.eulerAngles;//回転を取る

                var info = new TileInfo(position2, bound.max.x + Mathf.Abs(add_bound.min.x), rotation2, tile2, MAP_SPEED);
                list.Add(info);
            }

            foreach (var data in list)
            {
                var position = data.m_position;
                var rotation = data.m_rotation;

                if (position.x >= bound.min.x)
                {
                    tilemap.SetTile(position, data.m_tile);
                    Matrix4x4 matrix = Matrix4x4.TRS(Vector3Int.zero, Quaternion.Euler(rotation), Vector3.one);
                    tilemap.SetTransformMatrix(position, matrix);
                }
                position.x += 1;
            }
        }
        //タイルマップのオブジェクトに子オブジェクトがついていた場合、そのオブジェクトも生成してあげる
        for (int i = 0; i < StageMaps[num].transform.childCount; i++)
        {
            GameObject obj = StageMaps[num].transform.GetChild(i).gameObject;
            //生成ポジションの決定
            Vector3 InstPos = new Vector3(bound.max.x + (add_bound.max.x / 2) + obj.transform.position.x - MAP_SPEED, obj.transform.localPosition.y);

            GameObject Instobj = Instantiate(obj, InstPos, Quaternion.identity);
            Instobj.transform.parent = StageGrid.transform;
        }

        //tilemap.CompressBounds();
    }
}

