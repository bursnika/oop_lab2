<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" encoding="utf-8" indent="yes"/>

  <xsl:template match="/Library">
    <html>
      <head>
        <title>Кафедральна бібліотека - Каталог</title>
        <style>
          body {
            font-family: Arial, sans-serif;
            margin: 20px;
            background-color: #f5f5f5;
          }
          h1 {
            color: #2c3e50;
            text-align: center;
            border-bottom: 3px solid #3498db;
            padding-bottom: 10px;
          }
          h2 {
            color: #34495e;
            margin-top: 30px;
            border-left: 5px solid #3498db;
            padding-left: 10px;
          }
          .section {
            background-color: white;
            padding: 20px;
            margin: 20px 0;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
          }
          table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 15px;
          }
          th {
            background-color: #3498db;
            color: white;
            padding: 12px;
            text-align: left;
            font-weight: bold;
          }
          td {
            padding: 10px;
            border-bottom: 1px solid #ddd;
          }
          tr:hover {
            background-color: #f8f9fa;
          }
          .book-item, .reader-item, .borrow-item {
            margin-bottom: 20px;
            padding: 15px;
            border-left: 4px solid #3498db;
            background-color: #f8f9fa;
            border-radius: 4px;
          }
          .book-title {
            font-size: 18px;
            font-weight: bold;
            color: #2c3e50;
            margin-bottom: 5px;
          }
          .authors {
            color: #7f8c8d;
            font-style: italic;
            margin-bottom: 8px;
          }
          .info-label {
            font-weight: bold;
            color: #34495e;
          }
          .available {
            color: #27ae60;
            font-weight: bold;
          }
          .not-available {
            color: #e74c3c;
            font-weight: bold;
          }
          .status-active {
            color: #27ae60;
            font-weight: bold;
          }
          .status-inactive {
            color: #e74c3c;
            font-weight: bold;
          }
          .annotation {
            margin-top: 8px;
            padding: 10px;
            background-color: white;
            border-radius: 4px;
            font-style: italic;
          }
        </style>
      </head>
      <body>
        <h1>Кафедральна бібліотека</h1>

        <div class="section">
          <h2>Каталог книг</h2>
          <xsl:apply-templates select="Books"/>
        </div>

        <div class="section">
          <h2>Читачі</h2>
          <xsl:apply-templates select="Readers"/>
        </div>

        <div class="section">
          <h2>Видані книги</h2>
          <xsl:apply-templates select="BorrowedBooks"/>
        </div>
      </body>
    </html>
  </xsl:template>

  <xsl:template match="Books">
    <xsl:for-each select="Book">
      <div class="book-item">
        <div class="book-title">
          <xsl:value-of select="Title"/>
          <xsl:if test="@year">
            (<xsl:value-of select="@year"/>)
          </xsl:if>
        </div>

        <div class="authors">
          <xsl:for-each select="Author">
            <xsl:value-of select="FirstName"/>
            <xsl:text> </xsl:text>
            <xsl:if test="MiddleName">
              <xsl:value-of select="MiddleName"/>
              <xsl:text> </xsl:text>
            </xsl:if>
            <xsl:value-of select="LastName"/>
            <xsl:if test="position() != last()">
              <xsl:text>, </xsl:text>
            </xsl:if>
          </xsl:for-each>
        </div>

        <div>
          <span class="info-label">ISBN:</span>
          <xsl:text> </xsl:text>
          <xsl:value-of select="@isbn"/>
        </div>

        <div>
          <span class="info-label">Категорія:</span>
          <xsl:text> </xsl:text>
          <xsl:value-of select="Category"/>
        </div>

        <div>
          <span class="info-label">Видавництво:</span>
          <xsl:text> </xsl:text>
          <xsl:value-of select="Publisher"/>
          <xsl:text> | </xsl:text>
          <span class="info-label">Сторінок:</span>
          <xsl:text> </xsl:text>
          <xsl:value-of select="Pages"/>
        </div>

        <xsl:if test="@language">
          <div>
            <span class="info-label">Мова:</span>
            <xsl:text> </xsl:text>
            <xsl:value-of select="@language"/>
          </div>
        </xsl:if>

        <xsl:if test="@edition">
          <div>
            <span class="info-label">Видання:</span>
            <xsl:text> </xsl:text>
            <xsl:value-of select="@edition"/>
          </div>
        </xsl:if>

        <div>
          <span class="info-label">Доступність:</span>
          <xsl:text> </xsl:text>
          <xsl:choose>
            <xsl:when test="@available='true'">
              <span class="available">Доступна</span>
            </xsl:when>
            <xsl:when test="@available='false'">
              <span class="not-available">Видана</span>
            </xsl:when>
            <xsl:otherwise>
              <span>Інформація відсутня</span>
            </xsl:otherwise>
          </xsl:choose>
        </div>

        <div class="annotation">
          <xsl:value-of select="Annotation"/>
        </div>
      </div>
    </xsl:for-each>
  </xsl:template>

  <xsl:template match="Readers">
    <table>
      <tr>
        <th>ПІБ</th>
        <th>Факультет</th>
        <th>Кафедра</th>
        <th>Посада</th>
        <th>Email</th>
        <th>Статус</th>
      </tr>
      <xsl:for-each select="Reader">
        <tr>
          <td>
            <xsl:value-of select="LastName"/>
            <xsl:text> </xsl:text>
            <xsl:value-of select="FirstName"/>
            <xsl:text> </xsl:text>
            <xsl:value-of select="MiddleName"/>
          </td>
          <td><xsl:value-of select="Faculty"/></td>
          <td><xsl:value-of select="Department"/></td>
          <td>
            <xsl:value-of select="Position"/>
            <xsl:if test="Course">
              <xsl:text> (</xsl:text>
              <xsl:value-of select="Course"/>
              <xsl:text> курс)</xsl:text>
            </xsl:if>
          </td>
          <td><xsl:value-of select="Email"/></td>
          <td>
            <xsl:choose>
              <xsl:when test="@status='active'">
                <span class="status-active">Активний</span>
              </xsl:when>
              <xsl:otherwise>
                <span class="status-inactive">Неактивний</span>
              </xsl:otherwise>
            </xsl:choose>
            <xsl:if test="@membershipType">
              <xsl:text> (</xsl:text>
              <xsl:value-of select="@membershipType"/>
              <xsl:text>)</xsl:text>
            </xsl:if>
          </td>
        </tr>
      </xsl:for-each>
    </table>
  </xsl:template>

  <xsl:template match="BorrowedBooks">
    <table>
      <tr>
        <th>ID Позики</th>
        <th>ID Читача</th>
        <th>ID Книги</th>
        <th>Дата видачі</th>
        <th>Дата повернення</th>
        <th>Статус</th>
        <th>Примітки</th>
      </tr>
      <xsl:for-each select="Borrow">
        <tr>
          <td><xsl:value-of select="@borrowId"/></td>
          <td><xsl:value-of select="@readerId"/></td>
          <td><xsl:value-of select="@bookId"/></td>
          <td><xsl:value-of select="@borrowDate"/></td>
          <td>
            <xsl:choose>
              <xsl:when test="@returnDate">
                <xsl:value-of select="@returnDate"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="@dueDate"/>
                <xsl:text> (очікується)</xsl:text>
              </xsl:otherwise>
            </xsl:choose>
          </td>
          <td>
            <xsl:choose>
              <xsl:when test="@status='active'">
                <span class="status-active">Активна</span>
              </xsl:when>
              <xsl:when test="@status='returned'">
                <span>Повернено</span>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="@status"/>
              </xsl:otherwise>
            </xsl:choose>
            <xsl:if test="@renewable='true'">
              <xsl:text> | Можна продовжити</xsl:text>
            </xsl:if>
          </td>
          <td><xsl:value-of select="Notes"/></td>
        </tr>
      </xsl:for-each>
    </table>
  </xsl:template>

</xsl:stylesheet>
