const body = document.getElementsByTagName("body");
const bodyChildren = body[0].children;
var button = document.createElement("button");


for (let current = 0; current < bodyChildren.length; current++) {

    var button = document.createElement("button");

    //adding the parent id and the button id to the button
    button.setAttribute("parentId", bodyChildren[current].id);
    button.id = `popup_button-${current}`;

    //if the main parent is a container add button
    if (bodyChildren[current].tagName.toLowerCase() === "div" || bodyChildren[current].tagName.toLowerCase() === "section" || bodyChildren[current].tagName.toLowerCase() === "article") {
        bodyChildren[current].insertBefore(button, bodyChildren[current].firstChild);
        prepareButton(button.id);
        onHover(button.id);
        onClick(button.id);
        
    };
}

function onHover(id) {
    let button = document.getElementById(id);
    button.addEventListener("mouseenter", (event) => {
        event.target.style.opacity = "0.8";
    })
    button.addEventListener("mouseleave", (event) => {
        event.target.style.opacity = "1";
    })
}

function onClick(id) {
    let button = document.getElementById(id);
    let parentID = button.getAttribute("parentId");

    button.addEventListener("click", async (event) => {
      
        var container = document.getElementById(parentID);

        //if the main parent is a container send a request
        if (container.tagName.toLowerCase() === "div" || container.tagName.toLowerCase() === "section" || container.tagName.toLowerCase() === "article") {
            var flowUrl = "https://prod-16.centralindia.logic.azure.com:443/workflows/40ef1aa87a0f422785cba24efcef03bf/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=czW3ovz4bEeH00qSJ3rMRWZiSxDfA9MWWp1zal3Kou8";
            var attributeList = await readAttrbuitesFile('https://localhost:44396/json')
                .then(attrbuites => attrbuites)
                .catch(err => err);
            var destinationJSON = await readJSONFile("https://localhost:44384/json?url=" + window.location.href)
                .then(data => data)
                .catch(err => err);

            var dataToSend = JSON.stringify({
                "URL": window.location.href,
                "id": parentID,
                "attributeList": attributeList,
                "destinationJSON": destinationJSON,
            });
            console.log(dataToSend);
            var req = new XMLHttpRequest();

            //Request
            req.open("POST", flowUrl, true);
            req.setRequestHeader('Content-Type', 'application/json');

            //Response
            req.onreadystatechange = function () {
                if (this.readyState === 4) {
                    req.onreadystatechange = null;
                    if (this.status === 200) {
                        console.log("successful request..");

                    }
                    else if (this.status === 400) {
                        alert(this.statusText);
                        var result = this.response;
                        alert("Error" + result);
                    }
                }
            };

            //End
            req.send(dataToSend);
        } else {
            alert("click on the outer container");
        }
    })
}

function readAttrbuitesFile(url) {

    return fetch(url)
        .then((response) => {
            return response.json()
        })
        .then((data) => {
            // Work with JSON data here
            return data["attributesList"];
        })
        .catch((err) => {
            throw new Error(err);
        })
}

function readJSONFile(url) {
    return fetch(url)
        .then((response) => {
            return response.json()
        })
        .then((data) => {
            // Work with JSON data here
            return data;
        })
        .catch((err) => {
            throw new Error(err);
        })
}

function prepareButton(id) {
    let button = document.getElementById(id);
    button.className = "developers_button";
    button.innerHTML = "Change Values";
    button.style.border = "none";
    button.style.backgroundColor = "#555555";
    button.style.color = "#FFF";
    button.style.padding = "15px 32px";
    button.style.textAlign = "center";
    button.style.textDecoration = "none";
    button.style.display = "inline-block";
    button.style.fontSize = "16px";
    button.style.margin = "4px 2px";
    button.style.cursor = "pointer";
    button.style.width = "15%";
    button.style.height = "15%";
}