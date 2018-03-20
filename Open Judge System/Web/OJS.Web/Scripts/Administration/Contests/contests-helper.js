function makeContestNameLinkToDetailsPage(data) {
    var contestId = data.ContestId || data.Id;
    var contestName = data.ContestName || data.Name;

    if (typeof contestName == typeof undefined) {
        return null;
    }

    var nameUrl = contestName
        .replace('C#', 'CSharp')
        .replace('C++', 'CPlusPlus')
        .replace(/[^0-9a-zA-Z\-]/g, '-')
        .replace(/-{1,}/g, '-')
        .replace(/^-+|-+$/g, '');

    return "<a class='contest-name-link' href='/Contests/" + contestId + '/' + nameUrl + "' >" + contestName + "</a>";
}