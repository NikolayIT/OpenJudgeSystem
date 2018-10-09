function makeContestNameLinkToDetailsPage(data) {
    var contestId = data.ContestId || data.Id;
    var contestName = data.ContestName || data.Name;

    if (typeof contestName == typeof undefined) {
        return null;
    }

    var urlName = convertContestNameToUrlName(contestName);

    return "<a class='kendo-grid-link text-bold' href='/Contests/" + contestId + '/' + urlName + "' >" + contestName + "</a>";
}

function convertContestNameToUrlName(contestName) {
    return contestName
        .replace('C#', 'CSharp')
        .replace('C++', 'CPlusPlus')
        .replace(/[^0-9a-zA-Z\-]/g, '-')
        .replace(/-{1,}/g, '-')
        .replace(/^-+|-+$/g, '');
}