function makeContestNameLinkToDetailsPage(data) {
    var contestId = data.ContestId || data.Id;
    var contestName = data.ContestName || data.Name;

    var nameUrl = contestName
        .replace('C#', 'CSharp')
        .replace('C++', 'CPlusPlus')
        .replace(/[^0-9a-zA-Z\-]/g, '-')
        .replace(/-{1,}/g, '-')
        .replace(/^-+|-+$/g, '');

    return "<a class='text-primary' href='/Contests/" + contestId + '/' + nameUrl + "' >" + contestName + "</a>";
}