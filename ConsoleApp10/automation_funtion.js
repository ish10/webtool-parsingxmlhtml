const body = document.getElementsByTagName("body");

window.onload = () => {

    body[0].addEventListener("mouseover", event => {
        var container = document.getElementById(event.target.id);
        
        if (container.tagName.toLowerCase() == "div" || container.tagName.toLowerCase() == "section" || container.tagName.toLowerCase() == "article") {
            console.log(container);
            container.title = "click here to change values";
        }
        
    });

    body[0].addEventListener('click', (event) => {

        var elementID = event.target.id;
        var container = document.getElementById(elementID);
        if (container.tagName.toLowerCase() == "div" || container.tagName.toLowerCase() == "section" || container.tagName.toLowerCase() == "article") {
            var flowUrl = "https://prod-16.centralindia.logic.azure.com:443/workflows/40ef1aa87a0f422785cba24efcef03bf/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=czW3ovz4bEeH00qSJ3rMRWZiSxDfA9MWWp1zal3Kou8";
            var dataToSend = JSON.stringify({
                "URL": window.location.href,
                "id": elementID
            });
            console.log(dataToSend);
            readTextFile("D:\VS_project\Migration\webtool-parsingxmlhtml\WebScraping\XML\WebsiteConfig.json", function (text) {
                var data = JSON.parse(text);
                console.log(data);
            });
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
    });

    function readTextFile(file, callback) {
        var rawFile = new XMLHttpRequest();
        rawFile.overrideMimeType("application/json");
        rawFile.open("GET", file, true);
        rawFile.onreadystatechange = function () {
            if (rawFile.readyState === 4 && rawFile.status == "200") {
                callback(rawFile.responseText);
            }
        }
        rawFile.send(null);
    }

    
}