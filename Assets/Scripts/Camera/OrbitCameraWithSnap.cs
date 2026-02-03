using UnityEngine;
using System.Collections.Generic;

public class OrbitCameraWithSnap : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;

    [Header("Orbit Settings")]
    public float distance = 5.0f;
    public float rotationSpeed = 2.0f;
    public float zoomSpeed = 0.5f;
    public float minDistance = 2.0f;
    public float maxDistance = 15.0f;

    [Header("Vertical Limits")]
    public float minVerticalAngle = -20.0f;
    public float maxVerticalAngle = 80.0f;

    [Header("Snap Points Settings")]
    public bool enableSnapPoints = true;
    public float snapAngleThreshold = 5.0f; // آستانه برای اسنپ (درجه)
    public float snapSpeed = 5.0f; // سرعت اسنپ
    public List<SnapPoint> snapPoints = new List<SnapPoint>();

    [Header("Smoothness")]
    public bool smoothRotation = true;
    public float smoothTime = 0.3f;

    [Header("Mobile Settings")]
    public float mobileRotationMultiplier = 0.5f;
    public float mobileZoomMultiplier = 0.01f;
    public bool enablePinchZoom = true;
    public bool enableDragRotation = true;

    [System.Serializable]
    public class SnapPoint
    {
        public string name = "Snap Point";
        public float yawAngle = 0f; // زاویه افقی (درجه)
        public float pitchAngle = 30f; // زاویه عمودی (درجه)
        public float snapDistance = 5f; // فاصله در این نقطه
        public KeyCode hotkey = KeyCode.None; // کلید میانبر
    }

    // متغیرهای داخلی
    private float currentX = 0.0f;
    private float currentY = 0.0f;
    private Quaternion currentRotation;
    private bool isSnapping = false;
    private int currentSnapIndex = -1;
    private float snapProgress = 0f;

    // متغیرهای موبایل
    private Vector2?[] oldTouchPositions = { null, null };
    private float oldTouchDistance;

    // برای تشخیص پایان درگ
    private bool wasDragging = false;
    private Vector2 lastDragDelta = Vector2.zero;

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("لطفا یک هدف برای دوربین تنظیم کنید!");
            return;
        }

        // مقداردهی اولیه زوایا
        Vector3 angles = transform.eulerAngles;
        currentX = angles.y;
        currentY = angles.x;
        currentRotation = transform.rotation;

        // اگر نقاط اسنپ خالی است، نقاط پیش‌فرض اضافه کن
        if (snapPoints.Count == 0)
        {
            CreateDefaultSnapPoints();
        }
    }

    void CreateDefaultSnapPoints()
    {
        // ایجاد 4 نقطه اسنپ پیش‌فرض (چهار جهت اصلی)
        snapPoints.Add(new SnapPoint()
        {
            name = "Front",
            yawAngle = 0f,
            pitchAngle = 30f,
            snapDistance = 5f,
            hotkey = KeyCode.Alpha1
        });

        snapPoints.Add(new SnapPoint()
        {
            name = "Right",
            yawAngle = 90f,
            pitchAngle = 30f,
            snapDistance = 5f,
            hotkey = KeyCode.Alpha2
        });

        snapPoints.Add(new SnapPoint()
        {
            name = "Back",
            yawAngle = 180f,
            pitchAngle = 30f,
            snapDistance = 5f,
            hotkey = KeyCode.Alpha3
        });

        snapPoints.Add(new SnapPoint()
        {
            name = "Left",
            yawAngle = 270f,
            pitchAngle = 30f,
            snapDistance = 5f,
            hotkey = KeyCode.Alpha4
        });
    }

    void Update()
    {
        if (target == null) return;

        // کنترل‌های کیبورد برای اسنپ
        HandleKeyboardSnap();

        // تشخیص پلتفرم
        if (Application.isMobilePlatform || IsTouchDevice())
        {
            HandleMobileInput();
        }
        else
        {
            HandleDesktopInput();
        }

        // پردازش اسنپ
        ProcessSnap();
    }

    void LateUpdate()
    {
        if (target == null) return;
        UpdateCameraPosition();
    }

    void HandleDesktopInput()
    {
        bool isDragging = Input.GetMouseButton(0);

        if (isDragging)
        {
            // اگر کاربر در حال درگ است، اسنپ را متوقف کن
            if (isSnapping)
            {
                StopSnap();
            }

            currentX += Input.GetAxis("Mouse X") * rotationSpeed;
            currentY -= Input.GetAxis("Mouse Y") * rotationSpeed;
            currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);

            lastDragDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            wasDragging = true;
        }
        else if (wasDragging)
        {
            // اگر کاربر درگ را رها کرد، بررسی کن آیا باید اسنپ شود
            wasDragging = false;
            if (enableSnapPoints && Mathf.Abs(lastDragDelta.x) < 0.1f && Mathf.Abs(lastDragDelta.y) < 0.1f)
            {
                TrySnapToNearestPoint();
            }
        }

        // کنترل اسکرول
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            distance -= scroll * zoomSpeed * 50f;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    void HandleMobileInput()
    {
        // کنترل چرخش با درگ تک انگشتی
        if (enableDragRotation && Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                // اگر لمس شروع شد و در حال اسنپ بودیم، متوقفش کن
                if (isSnapping)
                {
                    StopSnap();
                }
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                currentX += touch.deltaPosition.x * rotationSpeed * mobileRotationMultiplier * Time.deltaTime;
                currentY -= touch.deltaPosition.y * rotationSpeed * mobileRotationMultiplier * Time.deltaTime;
                currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);

                lastDragDelta = touch.deltaPosition;
                wasDragging = true;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                // اگر لمس تمام شد و سرعت حرکت کم بود، اسنپ کن
                if (wasDragging && enableSnapPoints && lastDragDelta.magnitude < 10f)
                {
                    TrySnapToNearestPoint();
                }
                wasDragging = false;
            }
        }

        // کنترل زوم با پینچ
        if (enablePinchZoom && Input.touchCount == 2)
        {
            HandlePinchZoom();
        }
    }

    void HandlePinchZoom()
    {
        Touch touch1 = Input.GetTouch(0);
        Touch touch2 = Input.GetTouch(1);

        if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
        {
            oldTouchDistance = Vector2.Distance(touch1.position, touch2.position);
        }
        else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
        {
            float newTouchDistance = Vector2.Distance(touch1.position, touch2.position);
            float deltaDistance = (oldTouchDistance - newTouchDistance) * zoomSpeed * mobileZoomMultiplier;

            distance += deltaDistance;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);

            oldTouchDistance = newTouchDistance;
        }
    }

    void HandleKeyboardSnap()
    {
        // بررسی کلیدهای میانبر برای اسنپ
        for (int i = 0; i < snapPoints.Count; i++)
        {
            if (Input.GetKeyDown(snapPoints[i].hotkey))
            {
                SnapToPoint(i);
                break;
            }
        }

        // کلید اسنپ به نزدیک‌ترین نقطه (اختیاری)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TrySnapToNearestPoint();
        }

        // کلید لغو اسنپ
        if (Input.GetKeyDown(KeyCode.Escape) && isSnapping)
        {
            StopSnap();
        }
    }

    void TrySnapToNearestPoint()
    {
        if (!enableSnapPoints || snapPoints.Count == 0) return;

        float closestDistance = float.MaxValue;
        int closestIndex = -1;

        // پیدا کردن نزدیک‌ترین نقطه اسنپ
        for (int i = 0; i < snapPoints.Count; i++)
        {
            SnapPoint point = snapPoints[i];

            // محاسبه اختلاف زاویه
            float yawDiff = Mathf.DeltaAngle(currentX, point.yawAngle);
            float pitchDiff = Mathf.DeltaAngle(currentY, point.pitchAngle);

            // فاصله زاویه‌ای
            float angularDistance = Mathf.Sqrt(yawDiff * yawDiff + pitchDiff * pitchDiff);

            if (angularDistance < closestDistance)
            {
                closestDistance = angularDistance;
                closestIndex = i;
            }
        }

        // اگر به اندازه کافی نزدیک بود، اسنپ کن
        if (closestIndex != -1 && closestDistance <= snapAngleThreshold)
        {
            SnapToPoint(closestIndex);
        }
    }

    void SnapToPoint(int snapIndex)
    {
        if (snapIndex < 0 || snapIndex >= snapPoints.Count) return;

        currentSnapIndex = snapIndex;
        isSnapping = true;
        snapProgress = 0f;
    }

    void StopSnap()
    {
        isSnapping = false;
        currentSnapIndex = -1;
    }

    void ProcessSnap()
    {
        if (!isSnapping || currentSnapIndex == -1) return;

        SnapPoint targetPoint = snapPoints[currentSnapIndex];

        // افزایش پیشرفت اسنپ
        snapProgress += Time.deltaTime * snapSpeed;
        snapProgress = Mathf.Clamp01(snapProgress);

        // محاسبه مقادیر لرپ شده
        float t = smoothRotation ?
            Mathf.SmoothStep(0, 1, snapProgress) :
            snapProgress;

        // لرپ زاویه‌ها
        currentX = Mathf.LerpAngle(currentX, targetPoint.yawAngle, t);
        currentY = Mathf.LerpAngle(currentY, targetPoint.pitchAngle, t);

        // لرپ فاصله
        distance = Mathf.Lerp(distance, targetPoint.snapDistance, t);
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // اگر اسنپ کامل شد
        if (snapProgress >= 0.99f)
        {
            // تنظیم دقیق مقادیر نهایی
            currentX = targetPoint.yawAngle;
            currentY = targetPoint.pitchAngle;
            distance = targetPoint.snapDistance;
            isSnapping = false;

            Debug.Log($"Snapped to: {targetPoint.name}");
        }
    }

    void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);

        if (smoothRotation && !isSnapping)
        {
            currentRotation = Quaternion.Slerp(currentRotation, rotation,
                smoothTime * Time.deltaTime * 10f);
        }
        else
        {
            currentRotation = rotation;
        }

        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = currentRotation * negDistance + target.position;

        transform.rotation = currentRotation;
        transform.position = position;
        transform.LookAt(target);
    }

    // متدهای عمومی
    public void AddSnapPoint(float yaw, float pitch, float snapDistance, string name = "New Point")
    {
        snapPoints.Add(new SnapPoint()
        {
            name = name,
            yawAngle = yaw,
            pitchAngle = pitch,
            snapDistance = snapDistance
        });
    }

    public void SnapToPointByName(string pointName)
    {
        for (int i = 0; i < snapPoints.Count; i++)
        {
            if (snapPoints[i].name == pointName)
            {
                SnapToPoint(i);
                return;
            }
        }
        Debug.LogWarning($"Snap point with name '{pointName}' not found!");
    }

    public void SetSnapEnabled(bool enabled)
    {
        enableSnapPoints = enabled;
        if (!enabled && isSnapping)
        {
            StopSnap();
        }
    }

    bool IsTouchDevice()
    {
        return Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer;
    }

    // نمایش نقاط اسنپ در ادیتور
    void OnDrawGizmosSelected()
    {
        if (target == null) return;

        // رسم دایره مدار
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(target.position, distance);

        // رسم خط به هدف
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(target.position, transform.position);

        // نمایش نقاط اسنپ
        if (enableSnapPoints)
        {
            foreach (var snapPoint in snapPoints)
            {
                // محاسبه موقعیت نقطه اسنپ
                Quaternion rotation = Quaternion.Euler(snapPoint.pitchAngle, snapPoint.yawAngle, 0);
                Vector3 position = target.position + rotation * Vector3.back * snapPoint.snapDistance;

                // رسم نقطه
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(position, 0.2f);

                // رسم خط به هدف
                Gizmos.color = new Color(0, 1, 0, 0.5f);
                Gizmos.DrawLine(target.position, position);

                // نمایش نام نقطه
#if UNITY_EDITOR
                UnityEditor.Handles.Label(position + Vector3.up * 0.3f, snapPoint.name);
#endif
            }
        }
    }

    // متد برای UI (می‌توانید دکمه‌های UI بسازید)
    public void SnapToFront() => SnapToPointByName("Front");
    public void SnapToRight() => SnapToPointByName("Right");
    public void SnapToBack() => SnapToPointByName("Back");
    public void SnapToLeft() => SnapToPointByName("Left");
}
