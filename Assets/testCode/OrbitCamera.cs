using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target; // هدف دوربین
    public Vector3 targetOffset = Vector3.up; // افست برای موقعیت هدف

    [Header("Orbit Settings")]
    public float distance = 5f; // فاصله دوربین از هدف
    public float minDistance = 2f; // حداقل فاصله
    public float maxDistance = 10f; // حداکثر فاصله
    public float zoomSpeed = 0.5f; // سرعت زوم (پینچ)

    [Header("Rotation Settings")]
    public float xSpeed = 120f; // سرعت چرخش حول محور Y
    public float ySpeed = 120f; // سرعت چرخش حول محور X
    public float rotationSmoothing = 10f; // هموارسازی چرخش

    [Header("Angle Limits")]
    public float minYAngle = -20f; // حداقل زاویه عمودی
    public float maxYAngle = 80f; // حداکثر زاویه عمودی

    [Header("Position Points")]
    public Vector3[] orbitPoints; // نقاط مدنظر برای چرخش
    public float switchSpeed = 5f; // سرعت حرکت بین نقاط
    private int currentPointIndex = 0; // ایندکس نقطه فعلی

    // متغیرهای داخلی
    private float currentX = 0f;
    private float currentY = 0f;
    private float targetX = 0f;
    private float targetY = 0f;
    private float currentDistance;
    private float targetDistance;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isDragging = false;

    void Start()
    {
        // مقداردهی اولیه
        Vector3 angles = transform.eulerAngles;
        targetX = angles.y;
        targetY = angles.x;
        currentX = targetX;
        currentY = targetY;
        currentDistance = distance;
        targetDistance = distance;

        // اگر هدفی مشخص نشده، از دوربین فعلی استفاده کن
        if (target == null)
        {
            Debug.LogWarning("هیچ هدفی برای دوربین مشخص نشده است!");
        }

        // مقداردهی اولیه موقعیت هدف
        if (orbitPoints.Length > 0)
        {
            targetPosition = orbitPoints[0];
        }
    }

    void Update()
    {
        if (target == null)
            return;

        // مدیریت ورودی تاچ در موبایل
        HandleTouchInput();

        // مدیریت ورودی موس در ادیتور (برای تست)
        HandleMouseInput();

        // تغییر بین نقاط با صفحه کلید (برای تست)
        HandlePointSwitching();

        // به‌روزرسانی مقادیر هدف
        UpdateTargetValues();

        // اعمال تغییرات به دوربین
        ApplyCameraTransform();
    }

    void HandleTouchInput()
    {
        // اگر دو لمس داریم (پینچ برای زوم)
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            // تغییر فاصله با پینچ
            targetDistance += deltaMagnitudeDiff * zoomSpeed * Time.deltaTime;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }
        // اگر یک لمس داریم (درگ برای چرخش)
        else if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                isDragging = true;
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                // چرخش دوربین با درگ
                targetX += touch.deltaPosition.x * xSpeed * 0.02f;
                targetY -= touch.deltaPosition.y * ySpeed * 0.02f;
                targetY = ClampAngle(targetY, minYAngle, maxYAngle);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }
    }

    void HandleMouseInput()
    {
        // فقط در ادیتور فعال باشد
        if (!Application.isEditor) return;

        // زوم با اسکرول ماوس
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            targetDistance -= scroll * zoomSpeed * 10f;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }

        // چرخش با کلیک و درگ ماوس
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            targetX += Input.GetAxis("Mouse X") * xSpeed * 0.5f;
            targetY -= Input.GetAxis("Mouse Y") * ySpeed * 0.5f;
            targetY = ClampAngle(targetY, minYAngle, maxYAngle);
        }
    }

    void HandlePointSwitching()
    {
        // تغییر بین نقاط با کلیدهای کیبورد (برای تست)
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            SwitchToNextPoint();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            SwitchToPreviousPoint();
        }

        // یا با تاپ روی نقاط خاص در رابط کاربری (این بخش باید با UI مدیریت شود)
    }

    void UpdateTargetValues()
    {
        // هموارسازی مقادیر
        currentX = Mathf.Lerp(currentX, targetX, rotationSmoothing * Time.deltaTime);
        currentY = Mathf.Lerp(currentY, targetY, rotationSmoothing * Time.deltaTime);
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, rotationSmoothing * Time.deltaTime);

        // هموارسازی حرکت به سمت نقطه هدف
        if (orbitPoints.Length > 0)
        {
            Vector3 desiredPosition = target.position + targetOffset + orbitPoints[currentPointIndex];
            targetPosition = Vector3.Lerp(targetPosition, desiredPosition, switchSpeed * Time.deltaTime);
        }
        else
        {
            targetPosition = target.position + targetOffset;
        }
    }

    void ApplyCameraTransform()
    {
        // ایجاد کواترنیون برای چرخش
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);

        // محاسبه موقعیت جدید دوربین
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -currentDistance);
        Vector3 position = rotation * negDistance + targetPosition;

        // اعمال تغییرات
        transform.rotation = rotation;
        transform.position = position;
    }

    // متدهای عمومی برای کنترل از اسکریپت‌های دیگر
    public void SwitchToNextPoint()
    {
        if (orbitPoints.Length == 0) return;

        currentPointIndex = (currentPointIndex + 1) % orbitPoints.Length;
        Debug.Log($"Switched to point {currentPointIndex}");
    }

    public void SwitchToPreviousPoint()
    {
        if (orbitPoints.Length == 0) return;

        currentPointIndex = (currentPointIndex - 1 + orbitPoints.Length) % orbitPoints.Length;
        Debug.Log($"Switched to point {currentPointIndex}");
    }

    public void SwitchToPoint(int index)
    {
        if (orbitPoints.Length == 0 || index < 0 || index >= orbitPoints.Length)
        {
            Debug.LogError("Index out of range!");
            return;
        }

        currentPointIndex = index;
        Debug.Log($"Switched to point {currentPointIndex}");
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetDistance(float newDistance)
    {
        targetDistance = Mathf.Clamp(newDistance, minDistance, maxDistance);
    }

    // تابع کمکی برای محدود کردن زاویه
    float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }

    // برای نمایش گیزمو (در ادیتور)
    void OnDrawGizmosSelected()
    {
        if (target != null && orbitPoints.Length > 0)
        {
            Gizmos.color = Color.yellow;
            foreach (var point in orbitPoints)
            {
                Gizmos.DrawWireSphere(target.position + targetOffset + point, 0.3f);
            }

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(target.position + targetOffset + orbitPoints[currentPointIndex], 0.5f);
        }
    }
}
