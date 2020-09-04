
# Magic Data Common

[![Build status](https://travis-ci.org/polterguy/magic.data.common.svg?master)](https://travis-ci.org/polterguy/magic.data.common)

This is the generic data adapter, that transform dynamically from a lambda node structure, into SQL intended
to be executed towards your specific database implementation. If you wish to extend Magic to support a custom
database, this is the project you'd want to extend from, to make sure you keep the exact same structure as
you create your lambda objects, intended to be converted into SQL and executed towards some database
implementation.

Although the project is _not_ intended to be used directly, but rather through its special implementation,
such as the MySQL or MS SQL adapters - You can consume the project directly, and it does provide slots
for working directly with the generic adapter - Although, it will never actually execute the SQL,
but only allow you to generically parse a lambda object, producing SQL and SQL parameters in the process
for you. The project exposes the following slots.

* __[sql.create]__ - Creates an insert SQL for you, using the generic syntax for SQL
* __[sql.read]__ - Creates a select SQL for you, using the generic syntax for SQL
* __[sql.update]__ - Creates an update SQL for you, using the generic syntax for SQL
* __[sql.delete]__ - Creates a delete SQL for you, using the generic syntax for SQL

## [sql.create]

This slot requires one mandatory argument, being your table name. An example can be found below.

```
sql.read
   table:foo
```

The above will result in the following SQL returned to you.

```
select * from 'foo' limit 25
```

You can optionally supply the following arguments to this slot.

* __[where]__ - Where condition. Described further down, since it's common for all slots.
* __[columns]__ - Columns to select.
* __[order]__ - Which column to order the results by.
* __[direction]__ - Which direction to order your columns.

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
Notice, you can also create aggregate results, by simply adding your aggregate as your column, such as the
following illustrates.

```
sql.read
   table:table1
   columns
      count(*)
```

The above will result in the following SQL `select count(*) from 'table1' limit 25`.

## License

Although most of Magic's source code is Open Source, you will need a license key to use it.
[You can obtain a license key here](https://servergardens.com/buy/).
Notice, 7 days after you put Magic into production, it will stop working, unless you have a valid
license for it.

* [Get licensed](https://servergardens.com/buy/)
