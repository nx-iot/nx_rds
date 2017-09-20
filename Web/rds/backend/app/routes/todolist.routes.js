module.exports = function(app){
    var todolist = require('../controllers/todolist/todolist.controller')
    app.get('/',todolist.list)
    app.get('/:id',todolist.select)
    app.post('/',todolist.add)
    app.put('/',todolist.edit)
    app.delete('/:id',todolist.del)
    
}

