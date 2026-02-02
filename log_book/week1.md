# Week 1

## Refactoring


## `minitwit_tests.py`
For running the tests we ran into an issue that was that between python 2 and python 3 there has been a difference in how a string is represented. Before a string was represented as 'bytes', however it is in python 3 represented as a 'string' instead. This gives us a problem in the tests, as they in python 2 was written as:
```python
rv = self.register('user1', 'default')
assert 'The username is already taken' in rv.data
```
Here `rv.data` returns bytes, therefore in python 2, this comparison succeded, but not in python 3, because the comparison was then between a string and bytes. Therefore we fixed this by changing all the tests to:

```python
rv = self.register('user1', 'default')
assert 'The username is already taken' in rv.get_data(as_text=True)
```
Where `rv.get_data(as_text=True)`, then returns a string instead of bytes, and therefore this works. 

(Another fix could have been to change the strings to change the string to bytes, instead, however we went with this other way.)