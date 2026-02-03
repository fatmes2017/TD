using UnityEngine;

public class OrbitCameraMobile : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target; // هدفی که دوربین دور آن می‌چرخد

    [Header("Orbit Settings")]
    public float distance = 5.0f; // فاصله دوربین از هدف
    public float rotationSpeed = 2.0f; // سرعت چرخش
    public float zoomSpeed = 0.5f; // سرعت زوم با پینچ
    public float minDistance = 2.0f; // حداقل فاصله
    public float maxDistance = 15.0f; // حداکثر فاصله

    [Header("Vertical Limits")]
    public float minVerticalAngle = -20.0f; // حد پایین زاویه عمودی
    public float maxVerticalAngle = 80.0f; // حد بالای زاویه عمودی

    [Header("Smoothness")]
    public bool smoothRotation = true; // آیا چرخش نرم داشته باشیم؟
    public float smoothTime = 0.3f; // زمان نرم‌شدگی

    [Header("Mobile Settings")]
    public float mobileRotationMultiplier = 0.5f; // ضریب سرعت چرخش در موبایل
    public float mobileZoomMultiplier = 0.01f; // ضریب سرعت زوم در موبایل
    public bool enablePinchZoom = true; // فعال‌سازی زوم با پینچ
    public bool enableDragRotation = true; // فعال‌سازی چرخش با درگ

    // متغیرهای داخلی
    private float currentX = 0.0f;
    private float currentY = 0.0f;
    private Quaternion currentRotation;

    // متغیرهای مخصوص موبایل
    private Vector2?[] oldTouchPositions = { null, null };
    private float oldTouchDistance;

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("لطفا یک هدف برای دوربین تنظیم کنید!");
            return;
        }

        // مقداردهی اولیه زوایا بر اساس موقعیت اولیه دوربین
        Vector3 angles = transform.eulerAngles;
        currentX = angles.y;
        currentY = angles.x;

        currentRotation = transform.rotation;
    }

    void Update()
    {
        if (target == null) return;

        // تشخیص پلتفرم و اجرای کنترل مناسب
        if (Application.isMobilePlatform || IsTouchDevice())
        {
            HandleMobileInput();
        }
        else
        {
            HandleDesktopInput();
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // محاسبه موقعیت جدید دوربین
        UpdateCameraPosition();
    }

    void HandleDesktopInput()
    {
        // کنترل چرخش با درگ ماوس
        if (Input.GetMouseButton(0))
        {
            currentX += Input.GetAxis("Mouse X") * rotationSpeed;
            currentY -= Input.GetAxis("Mouse Y") * rotationSpeed;

            // محدودیت زاویه عمودی
            currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);
        }

        // کنترل اسکرول ماوس برای تغییر فاصله
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            distance -= scroll * zoomSpeed * 50f;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        // کنترل چرخش با کلیدهای کیبورد (اختیاری)
        float keyboardX = Input.GetAxis("Horizontal");
        float keyboardY = Input.GetAxis("Vertical");

        if (Mathf.Abs(keyboardX) > 0.1f || Mathf.Abs(keyboardY) > 0.1f)
        {
            currentX += keyboardX * rotationSpeed * Time.deltaTime * 50f;
            currentY += keyboardY * rotationSpeed * Time.deltaTime * 50f;
            currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);
        }
    }

    void HandleMobileInput()
    {
        // کنترل چرخش با درگ تک انگشتی
        if (enableDragRotation && Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Moved)
            {
                // چرخش دوربین با حرکت انگشت
                currentX += touch.deltaPosition.x * rotationSpeed * mobileRotationMultiplier * Time.deltaTime;
                currentY -= touch.deltaPosition.y * rotationSpeed * mobileRotationMultiplier * Time.deltaTime;

                // محدودیت زاویه عمودی
                currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);
            }
        }

        // کنترل زوم با پینچ دو انگشتی
        if (enablePinchZoom && Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
            {
                // ذخیره فاصله اولیه
                oldTouchDistance = Vector2.Distance(touch1.position, touch2.position);
            }
            else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
            {
                // محاسبه فاصله جدید
                float newTouchDistance = Vector2.Distance(touch1.position, touch2.position);

                // محاسبه تغییرات فاصله
                float deltaDistance = (oldTouchDistance - newTouchDistance) * zoomSpeed * mobileZoomMultiplier;

                // اعمال زوم
                distance += deltaDistance;
                distance = Mathf.Clamp(distance, minDistance, maxDistance);

                // به‌روزرسانی فاصله قبلی
                oldTouchDistance = newTouchDistance;
            }
        }

        // کنترل زوم با دو انگشت عمودی (اختیاری - جایگزین پینچ)
        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            // اگر هر دو انگشت به صورت عمودی حرکت کنند
            if (touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved)
            {
                // محاسبه میانگین حرکت عمودی
                float averageDeltaY = (touch1.deltaPosition.y + touch2.deltaPosition.y) * 0.5f;

                // اعمال زوم بر اساس حرکت عمودی
                distance -= averageDeltaY * zoomSpeed * mobileZoomMultiplier * 0.5f;
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
            }
        }
    }

    void UpdateCameraPosition()
    {
        // محاسبه چرخش
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);

        // اعمال نرم‌شدگی اگر فعال باشد
        if (smoothRotation)
        {
            currentRotation = Quaternion.Slerp(currentRotation, rotation,
                smoothTime * Time.deltaTime * 10f);
        }
        else
        {
            currentRotation = rotation;
        }

        // محاسبه موقعیت جدید دوربین
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = currentRotation * negDistance + target.position;

        // اعمال تغییرات به دوربین
        transform.rotation = currentRotation;
        transform.position = position;

        // نگاه کردن به سمت هدف
        transform.LookAt(target);
    }

    // تشخیص دستگاه لمسی
    bool IsTouchDevice()
    {
        return Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer;
    }

    // متدهای عمومی برای تنظیمات از بیرون
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetDistance(float newDistance)
    {
        distance = Mathf.Clamp(newDistance, minDistance, maxDistance);
    }

    public void SetRotationSpeed(float newSpeed)
    {
        rotationSpeed = Mathf.Max(0.1f, newSpeed);
    }

    // تنظیمات موبایل
    public void EnableMobileControls(bool enable)
    {
        enablePinchZoom = enable;
        enableDragRotation = enable;
    }

    // برای مشاهده دایره مدار در ادیتور
    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(target.position, distance);

            // رسم خط از هدف به دوربین
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(target.position, transform.position);
        }
    }

    // متد برای ریست کردن دوربین به موقعیت اولیه
    public void ResetCamera()
    {
        currentX = 0f;
        currentY = 0f;
        distance = Mathf.Lerp(minDistance, maxDistance, 0.5f);
    }
}
