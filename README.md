
# Magic Data Common

[![Build status](https://travis-ci.org/polterguy/magic.data.common.svg?master)](https://travis-ci.org/polterguy/magic.data.common)

This is the generic data adapter, that transform dynamically from a lambda node structure, into SQL, intended
to be executed towards your specific database implementation. If you wish to extend Magic to support a custom
database type, this is the project you'd want to extend from, to make sure you keep the exact same structure as
you create your lambda objects, intended to be converted into SQL, and executed towards some database
type. The project contains 4 base classes, which you should inherit from and extend to implement your custom logic.

* `SqlCreateBuilder` - Helper class to generate insert SQL statements.
* `SqlDeleteBuilder` - Helper class to generate delete SQL statements.
* `SqlReadBuilder` - Helper class to generate select SQL statements.
* `SqlUpdateBuilder` - Helper class to generate update SQL statements.

If you create your own database implementation, you'll need to inherit from the above classes, and override
whatever parts of these classes that doesn't by default work as your database type needs it to work.

Although the project is _not_ intended to be used directly, but rather through its special implementation,
such as the MySQL or MS SQL adapters - You can consume the project directly, and it does provide slots
for working directly with the generic adapter - Although, it will never actually execute the SQL,
but only allow you to generically parse a lambda object, producing generic SQL and SQL parameters in
the process for you. The project exposes the following slots.

* __[sql.create]__ - Creates an insert SQL for you, using the generic syntax for SQL.
* __[sql.read]__ - Creates a select SQL for you, using the generic syntax for SQL.
* __[sql.update]__ - Creates an update SQL for you, using the generic syntax for SQL.
* __[sql.delete]__ - Creates a delete SQL for you, using the generic syntax for SQL.

All of the above slots require you to pass in **[table]** as a mandatory argument, declaring which
table you intend to create your SQL towards. You can only supply _one_ table. The project does
not support joins.

## SQL injection attacks

The project protects you against SQL injection attacks, and values, and criteria, etc - But you should
_not_ allow the client to dynamically declare which columns to select, and/or field _names_ for
your `where` clauses. It will only protect your _values_, and _not_ table names, column names, etc.

## [sql.create]

This slot will generate the SQL necessary to insert a record into a database for you. It can only be given
one argument, which is __[values]__. Below is an example of usage.

```
sql.create
   table:table1
   values
      field1:howdy
      field2:world
```

Notice, to avoid SQL injection attacks, this slot will always return parameters expected to be passed in
from any potentially malicious clients as SQL parameters - Hence, the complete returned value of the
above Hyperlambda will be as follows.

```
sql.create:insert into 'table1' ('field1', 'field2') values (@0, @1)
   @0:howdy
   @1:world
```

The basica idea is that everything that might be dynamically injected into your data access layer,
should be consumed as `SqlParameters`, or something equivalent, to prevent SQL injection attacks
towards your database. This is true for all arguments passed in as data for all slots in the project.

## [sql.read]

This slot requires only one mandatory argument, being your table name. The slot creates a select
SQL statement for you. An example can be found below.

```
sql.read
   table:foo
```

The above will result in the following SQL returned to you. **Notice**, if you're using the special implementations,
such as e.g. **[mysql.read]** or **[mssql.read]** - The returned SQL might vary, according to your dialect. But the
results of executing the SQL will be the same.

```
select * from 'foo' limit 25
```

You can optionally supply the following arguments to this slot.

* __[columns]__ - Columns to select.
* __[order]__ - Which column to order the results by.
* __[direction]__ - Which direction to order your columns.
* __[limit]__ - How many records to return, default is 25. Set this value to -1 to avoid having the parser inject it.
* __[offset]__ - Offset of where to start returning records.
* __[where]__ - Where condition. Described further down, since it's common for all slots that supports `where` clauses.

For instance, to select only the _"field1"_ column and the _"field2"_ column from _"table1"_,
and ordering descending by _"field3"_ - You can use something resembling the following.

```
sql.read
   table:table1
   order:field3
   direction:desc
   columns
      field1
      field2
```

This will result in the following SQL returned `select 'field1','field2' from 'table1' order by 'field3' desc limit 25`.
**Notice**, you can also create aggregate results, by simply adding your aggregate as your column, such as the
following illustrates.

```
sql.read
   table:table1
   limit:-1
   columns
      count(*)
```

The above will result in the following SQL `select count(*) from 'table1'`. **Notice**, by setting **[limit]**
to _"-1"_, we avoid adding the limit parts to our SQL. Unless you explicitly specify a limit, the default value
will always be 25, to avoid accidentally exhausting your database, and/or server, by selecting all records from
a table with millions of records.

### Paging

To page your **[sql.read]** results, use **[limit]** and **[offset]**, such as the following illustrates.

```
sql.read
   table:table1
   offset:5
   limit:10
```

The above will return the following SQL `select * from 'table1' limit 10 offset 5`.

## [sql.update]

This slot allows you to update one or more records, in a specified **[table]**. Just like create, it requires
one mandatory argument, being **[values]**, implying columns/values you wish to update. This slot also takes
an optional **[where]** argument, which is described further down on this page. Its simplest version can be
imagined such as follows.

```
sql.update
   table:table1
   values
      field1:howdy
```

The above of course will result in the following.

```
sql.update:update 'table1' set 'field1' = @v0
   @v0:howdy
```

## License

Although most of Magic's source code is Open Source, you will need a license key to use it.
[You can obtain a license key here](https://servergardens.com/buy/).
Notice, 7 days after you put Magic into production, it will stop working, unless you have a valid
license for it.

* [Get licensed](https://servergardens.com/buy/)
