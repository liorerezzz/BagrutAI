async function askQuestion() {
    // שליפת הנושא
    const userQ = document.getElementById("userQ").value;

    // האובייקט שנשלח לשרת (השאילתה)
    const prompt = {
        "userQ": userQ,
    }

    const url = "./api/GPT/GPTChat"
    const params = {
        method: 'POST',
        headers: {
            Accept: 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(prompt) // המרה של האובייקט לג'ייסון
    }

    const response = await fetch(url, params);
    if (response.ok) {
        //המרה לפורמט מתאים
        let data = await response.json();
        data = JSON.parse(data);

        const questionsList = document.getElementById("answers");
        const question = document.createElement("li");
        //חילוץ הערכים
        const questionText = document.createTextNode("The question: " + userQ);
        const answerText = document.createTextNode("The answer: " + data["answer"]);
        //הצגה בHTML
        question.appendChild(questionText);
        question.appendChild(document.createElement("br"));
        question.appendChild(answerText);
        questionsList.appendChild(question);

    }
    else {
        console.log("Error communicating with server");
    }
}
async function getQuestion() {
    // שליפת הנושא
    const subject = document.getElementById("subject").value;

    // האובייקט שנשלח לשרת (השאילתה)
    const prompt = {
        "Subject": subject,
    }

    const url = "./api/GPT/GPTChat"
    const params = {
        method: 'POST',
        headers: {
            Accept: 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(prompt) // המרה של האובייקט לג'ייסון
    }

    const response = await fetch(url, params);
    if (response.ok) {
        //המרה לפורמט מתאים
        let data = await response.json();
        data = JSON.parse(data);

        const questionsList = document.getElementById("questions");
        const question = document.createElement("li");
        //חילוץ הערכים
        const questionText = document.createTextNode("The question: " + data["question"]);
        const answerText = document.createTextNode("The answer: " + data["answer"]);
        //הצגה בHTML
        question.appendChild(questionText);
        question.appendChild(document.createElement("br"));
        question.appendChild(answerText);
        questionsList.appendChild(question);

    }
    else {
        console.log("Error communicating with server");
    }
}

async function getImage() {
    const title = document.getElementById("title").value;

    const prompt = {
        "CourseTitle": title
    }

    const url = "./api/GPT/Dalle"
    const params = {
        method: 'POST',
        headers: {
            Accept: 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(prompt)
    }

    const response = await fetch(url, params);
    if (response.ok) {
        const data = await response.json();
        const image = document.getElementById("generatedImage");
        image.setAttribute("src", data);

    } else {
        console.log(await response.text());
    }
}