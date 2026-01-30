let isLoading = false;
let previousResponseId = null;
let fileId = null;

// פונקציה להעלאת קובץ PDF לשרת
async function uploadFile(e) {
    // מניעת טעינה בלחיצה על שליחה
    e.preventDefault();

    // בדיקה שקיים קובץ
    const fileInput = document.getElementById("pdfFile");
    const file = fileInput.files?.[0];

    if (!file) {
        showAlert("אנא העלו קובץ PDF.", "warning");
        return;
    }

    if (!isPdfFile(file)) {
        showAlert("הקובץ חייב להיות PDF.", "warning");
        return;
    }

    // הצגת טעינה וחסימת כפתור
    isLoading = true;
    setLoading(true);
    try {
        // יצירת אובייקט עם הקובץ
        const formData = new FormData();
        formData.append("File", file);

        const response = await fetch("api/GPT/FileUpload", {
            method: "POST",
            body: formData
        });

        const rawText = await response.text();

        if (!response.ok) {
            showAlert(`שגיאת שרת: ${rawText}`, "danger");
            return;
        }

        // שמירת מזהה הקובץ שהועלה
        const data = JSON.parse(rawText);
        if (data) {
            fileId = data;
            showAlert("הקובץ הועלה בהצלחה!", "success");
            document.getElementById("uploadFileForm").classList.add("d-none");
            document.getElementById("chatForm").classList.remove("d-none");
        }

    } catch (err) {
        console.error(err);
        showAlert("אירעה שגיאה בעת העלאת הקובץ. נסו שוב.", "danger");
    } finally {
        // בכל מקרה נסתיר את חלון הטעינה
        setLoading(false);
        isLoading = false;
    }
}

// פונקציה לשליחת שאלה לצ'אט
async function sendQuestion(e) {
    // מניעת טעינה בלחיצה על שליחה
    e.preventDefault();

    // בדיקה שיש טקסט בשאלה
    const questionInput = document.getElementById("question");
    const question = questionInput.value.trim();

    if (!question) {
        showAlert("אנא הקלידו שאלה.", "warning");
        return;
    }

    // בדיקה שהועלה קובץ
    if (!fileId) {
        showAlert("אנא העלו קובץ PDF לפני שליחת שאלה.", "warning");
        return;
    }

    // הוספת השאלה לצ'אט וריקון תיבת הטקסט
    addBubble("user", question);
    questionInput.value = "";

    // הצגת טעינה וחסימת כפתור
    isLoading = true;
    setLoading(true);

    try {
        // יצירת אובייקט JSON עם השאלה ומזהה הקובץ
        const requestBody = {
            Question: question,
            FileID: fileId
        };

        if (previousResponseId) {
            requestBody.PreviousResponseID = previousResponseId;
        }

        const response = await fetch("api/GPT/conversation", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(requestBody)
        });

        const rawText = await response.text();

        if (!response.ok) {
            // הצגת חלונית עם שגיאת שרת
            addBubble("bot", `שגיאת שרת: ${rawText}`);
            return;
        }

        // חילוץ התשובה
        let answer = rawText;
        const data = JSON.parse(rawText);
        if (data && data.text != null) {
            answer = data.text;
            previousResponseId = data.responseID;
        }

        addBubble("bot", answer);

    } catch (err) {
        console.error(err);
        addBubble("bot", "אירעה שגיאה בעת שליחת הבקשה. נסו שוב.");
    } finally {
        setLoading(false);
        isLoading = false;
    }
}
// הצגת שגיאה אם חסרים פרטים
function showAlert(message, type = "danger") {
    const el = document.getElementById("start");
    el.className = `alert alert-${type} border mt-2 mb-0`;
    el.textContent = message;
    el.classList.remove("d-none");
}

// הצגה/הסתרה של חלונית הטעינה וחסימה/הפעלה של הכפתור
function setLoading(loading) {
    const loadingDiv = document.getElementById("loading");
    loadingDiv.classList.toggle("d-none", !loading);
    document.getElementById("getDataBtn").disabled = loading;
}

// בדיקה שמדובר על .pdf
function isPdfFile(file) {
    if (!file) return false;
    const nameOk = file.name?.toLowerCase().endsWith(".pdf");
    const typeOk = file.type === "application/pdf" || file.type === ""; // some browsers send empty
    return nameOk && typeOk;
}
// הוספת בועת הצ'אט המתאימה - קביעת הצד בהתאם לתפקיד משתמש/צ'אט
// role: "user" | "bot"
function addBubble(role, text) {
    const list = document.getElementById("questions");

    const li = document.createElement("li");
    li.className = `d-flex ${role === "user" ? "justify-content-end" : "justify-content-start"}`;

    const bubble = document.createElement("div");
    bubble.className =
        "p-2 px-3 rounded-3 shadow-sm " +
        (role === "user" ? "bg-success text-white" : "bg-white border");
    bubble.style.maxWidth = "85%";
    bubble.style.whiteSpace = "pre-wrap";
    bubble.style.wordBreak = "break-word";
    bubble.textContent = String(text);

    li.appendChild(bubble);
    list.appendChild(li);

    // הסתרה של הודעת התחלה
    const answerDiv = document.getElementById("start");
    if (answerDiv) answerDiv.classList.add("d-none");

    // גלילה אוטומטית למטה
    const chatWindow = document.getElementById("chatWindow");
    if (chatWindow) chatWindow.scrollTop = chatWindow.scrollHeight;
}