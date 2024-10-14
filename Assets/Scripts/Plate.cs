using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Board
{
    public List<PlateData> board;
}

[System.Serializable]
public class PlateData
{
    public float x;
    public float y;
    public float pos_x;
    public float pos_y;
    public string image = "";
    public float image_x;
    public float image_y;
}

public class Plate : MonoBehaviour
{
    public GameObject obj;
    public GameObject imageObject;
    Vector3 dragStart;
    Vector2 initial_size;
    public GameObject imageSelectionTemplate;
    bool isMoving = false;
    float scroll_sensetivity = 1.25f;

    // Start is called before the first frame update
    void Start()
    {
        foreach (Sprite sprite in Resources.LoadAll<Sprite>("/"))
        {
            GameObject selectImage = Instantiate(imageSelectionTemplate);
            selectImage.GetComponent<Image>().sprite = sprite;
            selectImage.name = sprite.name;
            selectImage.transform.SetParent(imageSelectionTemplate.transform.parent);
            selectImage.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void create()
    {
        var new_plate = Instantiate(obj);
        new_plate.name = "Plate";
        new_plate.transform.SetParent(obj.transform.parent);
        new_plate.transform.position = obj.transform.position;
        new_plate.SetActive(true);
    }

    public void resize_start()
    {
        dragStart = Input.mousePosition;
        initial_size = obj.GetComponent<RectTransform>().sizeDelta;
    }

    public void resize()
    {
        if (Input.GetMouseButton(0)) //drag move.
        {
            Vector3 diff = Input.mousePosition - dragStart;

            print(dragStart + " " + Input.mousePosition + " " + diff);

            Vector2 size = initial_size;

            if (Mathf.Abs(diff.x) >= Mathf.Abs(diff.y))
            {
                size.x = Mathf.Clamp(size.x + diff.x, 50, 500);
                size.y = size.x;
            }
            else
            {
                size.y = Mathf.Clamp(size.y + (-diff.y), 50, 500);
                size.x = size.y;
            }
            Vector2 image_size = imageObject.GetComponent<RectTransform>().sizeDelta;
            obj.GetComponent<RectTransform>().sizeDelta = size;
            image_size.x = image_size.x * (size.x / image_size.x);
            image_size.y = image_size.x;
            imageObject.GetComponent<RectTransform>().sizeDelta = image_size;
        }
    }

    public void resizeImage()
    {
        Vector2 image_size = imageObject.GetComponent<RectTransform>().sizeDelta;
        image_size.x = Mathf.Clamp(image_size.x + Input.mouseScrollDelta.y * scroll_sensetivity, 25, obj.GetComponent<RectTransform>().sizeDelta.x);
        image_size.y = image_size.x;
        imageObject.GetComponent<RectTransform>().sizeDelta = image_size;
    }

    public void move()
    {
        isMoving = true;
        obj.transform.position = Input.mousePosition;
    }

    public void endMove() { isMoving = false; }

    public void remove()
    {
        Destroy(obj);
    }

    public void openImageSelection(GameObject selectionDialog)
    {
        selectionDialog.SetActive(!isMoving);
    }

    public void imageSelection(GameObject targetSpriteObject)
    {
        imageObject.GetComponent<Image>().sprite = targetSpriteObject.GetComponent<Image>().sprite;
    }

    public void savePlates()
    {
        List<PlateData> data = new List<PlateData>();
        string savePath = Application.persistentDataPath + "/PlateData.json";
        for (int i = 4; i < obj.transform.parent.childCount; i++)
        {
            GameObject plate = obj.transform.parent.GetChild(i).gameObject;
            PlateData plateData = new PlateData();

            //size
            plateData.x = plate.GetComponent<RectTransform>().sizeDelta.x;
            plateData.y = plate.GetComponent<RectTransform>().sizeDelta.y;

            //position
            plateData.pos_x = plate.transform.position.x;
            plateData.pos_y = plate.transform.position.y;

            plateData.image = plate.transform.GetChild(0).GetComponent<Image>().sprite.name; //child 0 is image object

            //image size
            plateData.image_x = plate.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x;
            plateData.image_y = plate.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y;

            data.Add(plateData);
        }
        Board board = new Board() { board = data };
        string savePlatesData = JsonUtility.ToJson(board, true);
        File.WriteAllText(savePath, savePlatesData);
    }

    public void loadPlates()
    {
        string savePath = Application.persistentDataPath + "/PlateData.json";
        Board board = new Board();

        if (File.Exists(savePath))
        {
            string loadBoardData = File.ReadAllText(savePath);
            board = JsonUtility.FromJson<Board>(loadBoardData);
        }
        else { return; }

        foreach(PlateData plateData in board.board)
        {
            var new_plate = Instantiate(obj);
            new_plate.name = "Plate";
            new_plate.transform.SetParent(obj.transform.parent);

            //position data
            var plate_position = new_plate.transform.position;
            plate_position.x = plateData.pos_x;
            plate_position.y = plateData.pos_y;
            new_plate.transform.position = plate_position;

            //size data
            var plate_size = new_plate.GetComponent<RectTransform>().sizeDelta;
            plate_size.x = plateData.x;
            plate_size.y = plateData.y;
            new_plate.GetComponent<RectTransform>().sizeDelta = plate_size;

            //image size data
            var image_size = new_plate.transform.GetChild(0).gameObject.GetComponent<RectTransform>().sizeDelta;
            image_size.x = plateData.image_x;
            image_size.y = plateData.image_y;
            new_plate.transform.GetChild(0).gameObject.GetComponent<RectTransform>().sizeDelta = image_size;

            new_plate.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(plateData.image);

            new_plate.SetActive(true);
        }
    }
}
