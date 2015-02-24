function(path, format, formatter) 
{ 
    var generated = db.getCollection('articles-generated');
    var formatted = formatter(db.articles.find({Path:path})[0].Markdown); 
    var existing = generated.find({ArticlePath:path,Format:format})[0];
    if (existing == undefined)
    {
        generated.insert(
        {
            ArticlePath:path,
            Format:format,
            GeneratedBy:'mongodb',
            GeneratedOn:new Date(),
            Content:formatted
        });
    }
    else{
        existing.GeneratedBy = 'mongodb';
        existing.GeneratedOn = new Date();
        existing.Content = formatted;
        generated.save(existing);
    }
   
}