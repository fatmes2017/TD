using UnityEngine;

public class MainCameraController : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target; // هدف اصلی دوربین
    [SerializeField] private Vector3 targetOffset = Vector3.zero;

    [Header("Orbit Points")]
    [SerializeField] private Transform[] orbitPoints; // نقاط دور چرخش
    private int currentPointIndex = 0;

    [Header("Movement Settings")]
    [SerializeField] private float rotationSpeed = 5f; // سرعت چرخش بین نقاط
    [SerializeField] private float switchSpeed = 2f; // سرعت سویچ بین نقاط
    [SerializeField] private float smoothTime = 0.3f; // نرمی حرکت
    private Vector3 velocity = Vector3.zero;

    [Header("Zoom Settings")]
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 15f;
    [SerializeField] private float zoomSpeed = 0.5f;
    private float currentZoom = 5f;

    [Header("Touch Settings")]
    [SerializeField] private float touchRotationSpeed = 0.5f;
    [SerializeField] private float touchZoomSpeed = 0.01f;
    [SerializeField] private float touchDeadZone = 10f; // منطقه مرده برای تشخیص درگ/پینچ

    // متغیرهای کنترل لمسی
    private Vector2[] lastTouchPositions = new Vector2[2];
    private bool isDragging = false;
    private bool isPinching = false;
    private float lastPinchDistance = 0f;

    private Quaternion targetRotation;
    private Vector3 targetPosition;

    private CameraControl camControl;
    //private void Awake()
    //{


    //    camControl = FindObjectOfType<CameraControl>();
    //    target = camControl.target;
    //    orbitPoints = camControl.GetCameraOrbitPoints();

    //}

    void Start()
    {
        if (orbitPoints.Length > 0)
        {
            transform.position = orbitPoints[currentPointIndex].position;
            transform.LookAt(target.position + targetOffset);
        }

        targetRotation = transform.rotation;
        currentZoom = Vector3.Distance(transform.position, target.position + targetOffset);
    }

    void Update()
    {
        if (target == null || orbitPoints.Length == 0) return;

        HandleTouchInput();
        HandleKeyboardInput();
        UpdateCamera();
    }

    void HandleTouchInput()
    {
        // تشخیص تعداد لمس‌ها
        int touchCount = Input.touchCount;

        if (touchCount == 0)
        {
            isDragging = false;
            isPinching = false;
            return;
        }

        // پینچ (دو انگشت) - زوم
        if (touchCount == 2)
        {
            isPinching = true;
            isDragging = false;

            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            if (touch2.phase == TouchPhase.Began)
            {
                lastPinchDistance = Vector2.Distance(touch1.position, touch2.position);
            }
            else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
            {
                float currentDistance = Vector2.Distance(touch1.position, touch2.position);
                float pinchDelta = (currentDistance - lastPinchDistance) * touchZoomSpeed;

                currentZoom = Mathf.Clamp(currentZoom - pinchDelta, minZoom, maxZoom);
                lastPinchDistance = currentDistance;
            }
        }
        // درگ (یک انگشت) - چرخش
        else if (touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    lastTouchPositions[0] = touch.position;
                    isDragging = true;
                    break;

                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        Vector2 delta = touch.position - lastTouchPositions[0];

                        // چک کردن منطقه مرده
                        if (Mathf.Abs(delta.x) > touchDeadZone || Mathf.Abs(delta.y) > touchDeadZone)
                        {
                            // چرخش حول محور Y
                            float rotationY = delta.x * touchRotationSpeed * Time.deltaTime;
                            RotateAroundTarget(rotationY);

                            // چرخش عمودی محدود شده
                            float rotationX = -delta.y * touchRotationSpeed * 0.5f * Time.deltaTime;
                            Vector3 currentEuler = transform.eulerAngles;
                            float newX = Mathf.Clamp(currentEuler.x + rotationX, 10f, 80f);
                            transform.eulerAngles = new Vector3(newX, currentEuler.y, currentEuler.z);
                        }

                        lastTouchPositions[0] = touch.position;
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isDragging = false;
                    break;
            }
        }
    }

    void HandleKeyboardInput()
    {
        // چرخش با کلیدهای چپ/راست
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            RotateAroundTarget(-rotationSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            RotateAroundTarget(rotationSpeed * Time.deltaTime);
        }

        // سویچ بین نقاط با کلیدهای بالا/پایین یا A/D
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.A))
        {
            SwitchToPreviousPoint();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.D))
        {
            SwitchToNextPoint();
        }

        // زوم با کلیدهای W/S
        if (Input.GetKey(KeyCode.W))
        {
            currentZoom = Mathf.Clamp(currentZoom - zoomSpeed * Time.deltaTime, minZoom, maxZoom);
        }
        if (Input.GetKey(KeyCode.S))
        {
            currentZoom = Mathf.Clamp(currentZoom + zoomSpeed * Time.deltaTime, minZoom, maxZoom);
        }
    }

    void RotateAroundTarget(float angle)
    {
        transform.RotateAround(target.position + targetOffset, Vector3.up, angle);
    }

    void UpdateCamera()
    {
        // به‌روزرسانی موقعیت بر اساس نقطه جاری
        Vector3 desiredPosition = orbitPoints[currentPointIndex].position;
        Vector3 directionToTarget = (target.position + targetOffset - desiredPosition).normalized;

        // اعمال زوم
        desiredPosition = (target.position + targetOffset) - directionToTarget * currentZoom;

        // حرکت نرم دوربین
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);

        // نگاه کردن به هدف
        Quaternion lookRotation = Quaternion.LookRotation((target.position + targetOffset) - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * switchSpeed);
    }

    // سویچ به نقطه بعدی
    public void SwitchToNextPoint()
    {
        if (orbitPoints.Length == 0) return;
        currentPointIndex = (currentPointIndex + 1) % orbitPoints.Length;
    }

    // سویچ به نقطه قبلی
    public void SwitchToPreviousPoint()
    {
        if (orbitPoints.Length == 0) return;
        currentPointIndex = (currentPointIndex - 1 + orbitPoints.Length) % orbitPoints.Length;
    }

    // تنظیم نقطه جاری توسط ایندکس
    public void SetCurrentPoint(int index)
    {
        if (index >= 0 && index < orbitPoints.Length)
        {
            currentPointIndex = index;
        }
    }

    // تنظیم نقطه جاری با ارجاع مستقیم
    public void SetCurrentPoint(Transform point)
    {
        for (int i = 0; i < orbitPoints.Length; i++)
        {
            if (orbitPoints[i] == point)
            {
                currentPointIndex = i;
                return;
            }
        }
    }

    // متدهای تنظیم سرعت
    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = Mathf.Max(0, speed);
    }

    public void SetSwitchSpeed(float speed)
    {
        switchSpeed = Mathf.Max(0, speed);
    }

    public void SetZoom(float zoom)
    {
        currentZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
    }

    // برای دیباگ در موبایل
    void OnGUI()
    {
#if UNITY_ANDROID || UNITY_IOS
        GUI.Label(new Rect(10, 10, 200, 30), $"Touch Count: {Input.touchCount}");
        GUI.Label(new Rect(10, 40, 200, 30), $"Current Point: {currentPointIndex}");
        GUI.Label(new Rect(10, 70, 200, 30), $"Zoom: {currentZoom:F2}");
#endif
    }
}
