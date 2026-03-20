"""
To run this test locally, the following dependencies have to be setup:

  * `pip install selenium`
  * `pip install pymysql`
  * `pip install pytest`

You also need to install geckodriver (a test-driver for Firefox). You should install this directly in the folder 
where this test is located (so inside ./tests):
  * `wget https://github.com/mozilla/geckodriver/releases/download/v0.32.0/geckodriver-v0.32.0-linux64.tar.gz`
  * `tar xzvf geckodriver-v0.32.0-linux64.tar.gz`
  * After extraction, the downloaded artifact can be removed: `rm geckodriver-v0.32.0-linux64.tar.gz`
  * You also need to make sure that Firefox is installed on your machine, as the tests need both Firefox and Geckodriver to run.

The tests have to be run against our running application. To do that, run `docker compose up minitwit` to get the application
up and running. Then the tests can be executed via: `pytest test_itu_minitwit_ui.py`. If you want to see the tests running 
in Firefox as they are being executed, you need to comment out the lines with `firefox_options.add_argument("--headless")`
in the tests below. This will ensure that a browser window is opened up, where you will be able to see how the tests are running.

The environment variables GUI_URL, DB_HOST, DB_PORT, DB_USER, DB_PWD, and DB_NAME have some default values that work when 
running the tests locally as explained above. The environment variables are set to some specific values in the compose-file
so that it works when you run the tests only via docker. Remember that Docker requires the tests to run in headless mode. 
Run the tests via Docker by running the command `docker compose run --rm uitests`. 
"""

import os

from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.common.keys import Keys
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from selenium.webdriver.firefox.service import Service
from selenium.webdriver.firefox.options import Options
import pymysql.cursors


GUI_URL = os.environ.get('GUI_URL', "http://localhost:5035/register")
DB_HOST = os.environ.get('DB_HOST', "localhost")
DB_PORT = os.environ.get('DB_PORT', "3306")
DB_USER = os.environ.get('DB_USER', "root")
DB_PWD = os.environ.get('DB_PWD', "root")
DB_NAME = os.environ.get('DB_NAME', "minitwit")

def _register_user_via_gui(driver, data):
    driver.get(GUI_URL)

    wait = WebDriverWait(driver, 5)
    buttons = wait.until(EC.presence_of_all_elements_located((By.CLASS_NAME, "actions")))
    input_fields = driver.find_elements(By.TAG_NAME, "input")

    for idx, str_content in enumerate(data):
        input_fields[idx].send_keys(str_content)
    input_fields[4].send_keys(Keys.RETURN)

    wait = WebDriverWait(driver, 5)
    flashes = wait.until(EC.presence_of_all_elements_located((By.CLASS_NAME, "flashes")))

    return flashes


def _get_user_by_name(connection, name):
    with connection.cursor() as cursor:
        sql = "SELECT username FROM user WHERE username=%s"
        cursor.execute(sql, (name,))
        return cursor.fetchone()

def _delete_user_by_name(connection, name):
    with connection.cursor() as cursor:
        sql = "DELETE FROM user WHERE username=%s"
        cursor.execute(sql, (name,))


def test_register_user_via_gui():
    """
    This is a UI test. It only interacts with the UI that is rendered in the browser and checks that visual
    responses that users observe are displayed.
    """
    firefox_options = Options()
    # Make sure that headless argument is enabled, this is the only way it can run through docker
    firefox_options.add_argument("--headless")
    # firefox_options = None
    with webdriver.Firefox(service=Service("./geckodriver"), options=firefox_options) as driver:
        generated_msg = _register_user_via_gui(driver, ["Me", "me@some.where", "secure123", "secure123"])[0].text
        expected_msg = "You were successfully registered and can login now"
        assert generated_msg == expected_msg

    connection = pymysql.connect(host = DB_HOST,
                                port = int(DB_PORT),
                                user = DB_USER,
                                password= DB_PWD,
                                database=DB_NAME,
                                ssl_disabled=True,
                                cursorclass=pymysql.cursors.DictCursor)
    with connection:
        _delete_user_by_name(connection, 'Me')
        connection.commit()


def test_register_user_via_gui_and_check_db_entry():
    """
    This is an end-to-end test. Before registering a user via the UI, it checks that no such user exists in the
    database yet. After registering a user, it checks that the respective user appears in the database.
    """
    firefox_options = Options()
    # Make sure that headless argument is enabled, this is the only way it can run through docker
    firefox_options.add_argument("--headless")
    # firefox_options = None
    with webdriver.Firefox(service=Service("./geckodriver"), options=firefox_options) as driver:

        connection = pymysql.connect(host = DB_HOST,
                                port = int(DB_PORT),
                                user = DB_USER,
                                password= DB_PWD,
                                database=DB_NAME,
                                ssl_disabled=True,
                                cursorclass=pymysql.cursors.DictCursor)

        with connection:
            # Make sure that there is no user in the database with this name already
            assert _get_user_by_name(connection, "Me") == None

            generated_msg = _register_user_via_gui(driver, ["Me", "me@some.where", "secure123", "secure123"])[0].text
            expected_msg = "You were successfully registered and can login now"
            assert generated_msg == expected_msg

            # Make sure that the new user is commited to the database so that we can see the changes
            connection.commit()

            # Verify that the user now exists in the database
            assert _get_user_by_name(connection, "Me")["username"] == "Me"

            # We delete the created user from the database so the test can be run multiple times without 
            # throwing errors that user already exists. 
            _delete_user_by_name(connection, 'Me')
            connection.commit()

