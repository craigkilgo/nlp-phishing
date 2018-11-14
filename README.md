# nlp-phishing

### proc
The proc directory has a dotnet core program which generates the entire corpus.

The phishing dataset comes partially from https://www.phishtank.com/.  The entire json is too large to store in a repository.

### splitdata
Splitdata takes the output of the proc program and splits it randomly into a testing and training set.

### model
The model directory then generates the model based on those 2 sets of data.

