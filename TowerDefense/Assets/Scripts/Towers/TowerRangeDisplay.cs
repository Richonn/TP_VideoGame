using UnityEngine;

[RequireComponent(typeof(Tower))]
public class TowerRangeDisplay : MonoBehaviour
{
    [Header("Appearance")]
    [SerializeField] private Color color = new Color(1f, 1f, 0.2f, 0.5f);
    [SerializeField] private float lineWidth = 0.06f;

    [Header("Dashes")]
    [SerializeField] private int dashCount = 20;
    [SerializeField] [Range(0.1f, 0.9f)] private float dashRatio = 0.55f;
    [SerializeField] private int pointsPerDash = 8;

    void Start()
    {
        CreateDashedCircle();
    }

    public void RefreshDisplay()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);
        CreateDashedCircle();
    }

    private void CreateDashedCircle()
    {
        float radius = GetComponent<Tower>().range;
        float periodAngle = 360f / dashCount;
        float dashAngle = periodAngle * dashRatio;

        Material mat = new Material(Shader.Find("Sprites/Default"));

        for (int i = 0; i < dashCount; i++)
        {
            GameObject go = new GameObject($"RangeDash_{i}");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;

            LineRenderer lr = go.AddComponent<LineRenderer>();
            SetupLineRenderer(lr, mat);

            float startAngle = i * periodAngle;
            float endAngle = startAngle + dashAngle;

            lr.positionCount = pointsPerDash;
            for (int j = 0; j < pointsPerDash; j++)
            {
                float t = j / (float)(pointsPerDash - 1);
                float a = Mathf.Lerp(startAngle, endAngle, t) * Mathf.Deg2Rad;
                lr.SetPosition(j, new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f));
            }
        }
    }

    private void SetupLineRenderer(LineRenderer lr, Material mat)
    {
        lr.useWorldSpace = false;
        lr.loop = false;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.material = mat;
        lr.startColor = color;
        lr.endColor = color;
        lr.sortingOrder = 4;
    }
}
